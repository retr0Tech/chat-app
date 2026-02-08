namespace ChatApp.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBot { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;
}
