using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence;

public class MessageRepository : IMessageRepository
{
    private readonly ChatDbContext _db;

    public MessageRepository(ChatDbContext db) => _db = db;

    public async Task<Message> AddAsync(Message message)
    {
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();
        return message;
    }

    public async Task<IEnumerable<Message>> GetRecentByRoomAsync(int chatRoomId, int count = 50)
    {
        return await _db.Messages
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}
