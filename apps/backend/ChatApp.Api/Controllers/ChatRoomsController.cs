using ChatApp.Api.Models;
using ChatApp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatRoomsController : ControllerBase
{
    private readonly IChatRoomRepository _chatRoomRepo;
    private readonly IMessageRepository _messageRepo;

    public ChatRoomsController(IChatRoomRepository chatRoomRepo, IMessageRepository messageRepo)
    {
        _chatRoomRepo = chatRoomRepo;
        _messageRepo = messageRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _chatRoomRepo.GetAllAsync();
        return Ok(rooms.Select(r => new ChatRoomResponse(r.Id, r.Name)));
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var room = await _chatRoomRepo.GetByIdAsync(id);
        if (room == null) return NotFound();

        var messages = await _messageRepo.GetRecentByRoomAsync(id);
        return Ok(messages.Select(m => new MessageResponse(
            m.Id, m.ChatRoomId, m.Username, m.Content, m.CreatedAt, m.IsBot
        )));
    }
}
