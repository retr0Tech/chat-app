using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces;

public interface IChatRoomRepository
{
    Task<IEnumerable<ChatRoom>> GetAllAsync();
    Task<ChatRoom?> GetByIdAsync(int id);
    Task<ChatRoom> AddAsync(ChatRoom chatRoom);
}
