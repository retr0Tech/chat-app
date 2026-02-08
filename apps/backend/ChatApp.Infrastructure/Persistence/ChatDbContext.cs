using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence;

public class ChatDbContext : IdentityDbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatRoom>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(100);
            e.HasData(
                new ChatRoom { Id = 1, Name = "General" },
                new ChatRoom { Id = 2, Name = "Technology" }
            );
        });

        builder.Entity<Message>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Content).IsRequired().HasMaxLength(2000);
            e.Property(m => m.Username).IsRequired().HasMaxLength(100);
            e.HasIndex(m => new { m.ChatRoomId, m.CreatedAt });
            e.HasOne(m => m.ChatRoom)
             .WithMany(r => r.Messages)
             .HasForeignKey(m => m.ChatRoomId);
        });
    }
}
