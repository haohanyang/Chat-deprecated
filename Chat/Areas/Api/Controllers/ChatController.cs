using System.Security.Claims;
using Chat.Areas.Api.Services;
using Chat.Common;
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Areas.Api.Controllers;

[Route("api/chats")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<ChatController> _logger;
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;

    public ChatController(IHubContext<ChatHub, IChatClient> hubContext,
        ILogger<ChatController> logger, IUserService userService, IGroupService groupService, IMessageService messageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _messageService = messageService;
        _userService = userService;
        _groupService = groupService;
    }

    /// <summary>
    /// Get all messages between the current user and another user with the given username
    /// </summary>
    [Authorize]
    [HttpGet("user/{username1}/{username2}")]
    public async Task<IActionResult> GetUserChat([FromRoute(Name = "username1")] string username1,
        [FromRoute(Name = "username2")] string username2)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != username1 && username != username2)
            return BadRequest("You can only get your own chat");
        try
        {
            var messages = await _messageService.GetUserChat(username1, username2);
            return Ok(messages);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get user chat between {} {} with unexpected error: {}", username1, username2, e.Message);
            return BadRequest("Unexpected error");
        }
    }

    /// <summary>
    /// Get all messages of the group
    /// </summary>
    [Authorize]
    [HttpGet("group/{group_id:int}")]
    public async Task<IActionResult> GetGroupChat([FromRoute(Name = "group_id")] int id)
    {
        try
        {
            var messages = await _messageService.GetGroupChat(id);
            return Ok(messages);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get group chat for group {} with unexpected error: {}", id, e.Message);
            return BadRequest("Unexpected error");
        }
    }

    /// <summary>
    /// Send a message to another user
    /// </summary>
    [Authorize]
    [HttpPost("user/message")]
    public async Task<IActionResult> SendUserMessage([FromBody] UserMessageDTO message)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != message.Sender.Username)
            return BadRequest("You can only send messages as yourself");
        try
        {
            var receiver = await _userService.GetUser(message.Receiver.Username);
            await _hubContext.Clients.User(message.Receiver.Username).ReceiveUserMessage(message);
            var messageId = await _messageService.SaveMessage(message);
            _logger.LogInformation("User {} sent a message to user {}, message id {}", username,
                message.Receiver, messageId);
            return CreatedAtAction(nameof(SendUserMessage), new { Id = messageId });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to send message from {} to {} with the unexpected error: {}",
                username, message.Receiver.Username, e.Message);
            return BadRequest("Unexpected error");
        }

    }

    /// <summary>
    /// Send a message to a group
    /// </summary>
    [Authorize]
    [HttpPost("group/message")]
    public async Task<ActionResult> SendGroupMessage([FromBody] GroupMessageDTO message)
    {

        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != message.Sender.Username)
            return BadRequest("You can only send messages as yourself");
        try
        {
            var groupName = message.Receiver.Name;
            var members = await _groupService.GetGroupMembers(message.Receiver.Id);

            if (!members.Select(m => m.Username).Contains(username))
                return BadRequest($"You are not in the group {groupName}");

            await _hubContext.Clients.Group(groupName).ReceiveGroupMessage(message);
            var messageId = await _messageService.SaveMessage(message);

            _logger.LogInformation("User {} sent a message to group {}, message id {}", username,
                groupName, messageId);

            return CreatedAtAction(nameof(SendGroupMessage), new { Id = messageId });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to send message with the unexpected error error: {}", e.Message);
            return BadRequest("Unexpected error");
        }
    }
}