using System.Text;
using System.Text.Json;
using ChatApp.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ChatApp.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessageBrokerPublisher, IAsyncDisposable
{
    public const string StockCommandQueue = "stock_commands";
    public const string BotResponseQueue = "bot_responses";

    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqPublisher(IConnection connection, IChannel channel, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<RabbitMqPublisher> CreateAsync(IConfiguration config, ILogger<RabbitMqPublisher> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
            UserName = config["RabbitMQ:UserName"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(StockCommandQueue, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueDeclareAsync(BotResponseQueue, durable: true, exclusive: false, autoDelete: false);

        return new RabbitMqPublisher(connection, channel, logger);
    }

    public async Task PublishStockCommandAsync(string stockCode, int chatRoomId)
    {
        var payload = JsonSerializer.Serialize(new { StockCode = stockCode, ChatRoomId = chatRoomId });
        await PublishAsync(StockCommandQueue, payload);
        _logger.LogInformation("Published stock command: {StockCode} for room {RoomId}", stockCode, chatRoomId);
    }

    public async Task PublishBotResponseAsync(string message, int chatRoomId)
    {
        var payload = JsonSerializer.Serialize(new { Message = message, ChatRoomId = chatRoomId });
        await PublishAsync(BotResponseQueue, payload);
        _logger.LogInformation("Published bot response for room {RoomId}", chatRoomId);
    }

    private async Task PublishAsync(string queue, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties { Persistent = true };
        await _channel.BasicPublishAsync("", queue, false, props, body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}
