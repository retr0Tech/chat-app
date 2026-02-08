namespace ChatApp.Api.Models;

public record SendMessageRequest(int ChatRoomId, string Content);

public record MessageResponse(
    int Id,
    int ChatRoomId,
    string Username,
    string Content,
    DateTime CreatedAt,
    bool IsBot
);

public record ChatRoomResponse(int Id, string Name);
