using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly ChatDbContext _db;

    public ChatRoomRepository(ChatDbContext db) => _db = db;

    public async Task<IEnumerable<ChatRoom>> GetAllAsync()
    {
        return await _db.ChatRooms.ToListAsync();
    }

    public async Task<ChatRoom?> GetByIdAsync(int id)
    {
        return await _db.ChatRooms.FindAsync(id);
    }

    public async Task<ChatRoom> AddAsync(ChatRoom chatRoom)
    {
        _db.ChatRooms.Add(chatRoom);
        await _db.SaveChangesAsync();
        return chatRoom;
    }
}
