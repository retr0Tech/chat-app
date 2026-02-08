using System.Text;
using System.Text.Json;
using ChatApp.Infrastructure.ExternalApis;
using ChatApp.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatApp.Bot.Consumers;

public class StockCommandConsumer : BackgroundService
{
    private readonly StooqClient _stooqClient;
    private readonly IConfiguration _config;
    private readonly ILogger<StockCommandConsumer> _logger;

    public StockCommandConsumer(
        StooqClient stooqClient,
        IConfiguration config,
        ILogger<StockCommandConsumer> logger)
    {
        _stooqClient = stooqClient;
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

        // Declare both queues
        await channel.QueueDeclareAsync(
            RabbitMqPublisher.StockCommandQueue,
            durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);
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
                var command = JsonSerializer.Deserialize<StockCommand>(body);

                if (command == null)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation("Processing stock command: {StockCode} for room {RoomId}",
                    command.StockCode, command.ChatRoomId);

                // Call Stooq API and parse CSV
                var quoteMessage = await _stooqClient.GetStockQuoteAsync(command.StockCode);

                // Publish bot response back
                var response = JsonSerializer.Serialize(new
                {
                    Message = quoteMessage,
                    command.ChatRoomId
                });

                var responseBody = Encoding.UTF8.GetBytes(response);
                var props = new BasicProperties { Persistent = true };
                await channel.BasicPublishAsync("", RabbitMqPublisher.BotResponseQueue, false, props, responseBody);

                _logger.LogInformation("Published bot response: {Message}", quoteMessage);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock command");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(RabbitMqPublisher.StockCommandQueue, false, consumer, stoppingToken);

        // Keep running until cancelled
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private record StockCommand(string StockCode, int ChatRoomId);
}
