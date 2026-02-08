using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message);
    Task<IEnumerable<Message>> GetRecentByRoomAsync(int chatRoomId, int count = 50);
}
