using System.Security.Claims;
using ChatApp.Api.Models;
using ChatApp.Api.Services;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageRepository _messageRepo;
    private readonly IMessageBrokerPublisher _publisher;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IMessageRepository messageRepo,
        IMessageBrokerPublisher publisher,
        ILogger<ChatHub> logger)
    {
        _messageRepo = messageRepo;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task JoinRoom(int chatRoomId)
    {
        var groupName = $"room_{chatRoomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {User} joined room {Room}", Context.User?.Identity?.Name, chatRoomId);
    }

    public async Task LeaveRoom(int chatRoomId)
    {
        var groupName = $"room_{chatRoomId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessage(int chatRoomId, string content)
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        // Check if it's a stock command
        if (CommandParser.IsStockCommand(content))
        {
            var stockCode = CommandParser.ExtractStockCode(content);
            if (stockCode != null)
            {
                _logger.LogInformation("Stock command detected: {StockCode}", stockCode);
                await _publisher.PublishStockCommandAsync(stockCode, chatRoomId);
                return; // Don't save command as a message
            }
        }

        // Regular message — save and broadcast
        var message = new Message
        {
            ChatRoomId = chatRoomId,
            UserId = userId,
            Username = username,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsBot = false
        };

        await _messageRepo.AddAsync(message);

        var response = new MessageResponse(
            message.Id, message.ChatRoomId, message.Username,
            message.Content, message.CreatedAt, message.IsBot
        );

        await Clients.Group($"room_{chatRoomId}").SendAsync("ReceiveMessage", response);
    }
}
