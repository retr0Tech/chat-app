using System.Text;
using System.Text.Json;
using ChatApp.Api.Hubs;
using ChatApp.Api.Models;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using ChatApp.Infrastructure.Messaging;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatApp.Api.Messaging;

public class BotResponseConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IConfiguration _config;
    private readonly ILogger<BotResponseConsumer> _logger;

    public BotResponseConsumer(
        IServiceScopeFactory scopeFactory,
        IHubContext<ChatHub> hubContext,
        IConfiguration config,
        ILogger<BotResponseConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
            UserName = _config["RabbitMQ:UserName"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            RabbitMqPublisher.BotResponseQueue,
            durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var data = JsonSerializer.Deserialize<BotResponseMessage>(body);

                if (data != null)
                {
                    // Save bot message to DB
                    using var scope = _scopeFactory.CreateScope();
                    var messageRepo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

                    var message = new Message
                    {
                        ChatRoomId = data.ChatRoomId,
                        UserId = "bot",
                        Username = "StockBot",
                        Content = data.Message,
                        CreatedAt = DateTime.UtcNow,
                        IsBot = true
                    };

                    await messageRepo.AddAsync(message);

                    // Broadcast to SignalR group
                    var response = new MessageResponse(
                        message.Id, message.ChatRoomId, message.Username,
                        message.Content, message.CreatedAt, message.IsBot
                    );

                    await _hubContext.Clients.Group($"room_{data.ChatRoomId}")
                        .SendAsync("ReceiveMessage", response, stoppingToken);

                    _logger.LogInformation("Bot message broadcast to room {RoomId}", data.ChatRoomId);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bot response");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(RabbitMqPublisher.BotResponseQueue, false, consumer, stoppingToken);

        // Keep running until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private record BotResponseMessage(string Message, int ChatRoomId);
}
