namespace ChatApp.Domain.Interfaces;

public interface IMessageBrokerPublisher
{
    Task PublishStockCommandAsync(string stockCode, int chatRoomId);
    Task PublishBotResponseAsync(string message, int chatRoomId);
}
