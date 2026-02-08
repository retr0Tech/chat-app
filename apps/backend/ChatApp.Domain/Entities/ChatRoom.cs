namespace ChatApp.Domain.Entities;

public class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
