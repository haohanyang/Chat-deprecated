using System.Security.Claims;
using Chat.Areas.Api.Services;
using Chat.Common;
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Areas.Api.Controllers;

[Route("api/message")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageService _messageService;
    private readonly IGroupService _groupService;
    private readonly IUserService _userService;

    public MessageController(IHubContext<ChatHub, IChatClient> hubContext,
        ILogger<MessageController> logger, IGroupService groupService, IUserService userService, IMessageService messageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _groupService = groupService;
        _messageService = messageService;
        _userService = userService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> AllMessages([FromQuery(Name = "username")] string username)
    {
        try
        {
            var messages = await _messageService.GetAllMessages(username);
            return Ok(messages);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get all messages with unexpected error:{}" + e.Message);
            return BadRequest("Unexpected error");
        }
    }

    [Authorize]
    [HttpGet("between")]
    public async Task<IActionResult> MessagesBetween([FromQuery(Name = "user")] string contact)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var messages = await _messageService.GetAllMessagesBetween(username, contact);
            return Ok(messages);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("MessagesBetween failed with unexpected error:{}" + e.Message);
            return BadRequest("Unexpected error");
        }
    }

    [HttpPost("send_test")]
    public async Task<ActionResult> SendTestMessage([FromQuery(Name = "username")] string username)
    {
        try
        {
            var message = new MessageDTO { Content = "test" };
            await _hubContext.Clients.User(username).ReceiveMessage(message);
            return Ok("ok");
        }
        catch (Exception e)
        {
            _logger.LogError("SendTestMessage failed with unexpected error : {}", e.Message);
            return BadRequest("Unexpected error");
        }
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<ActionResult> SendMessage([FromBody] MessageDTO message)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            if (message.Type == MessageType.GroupMessage)
            {
                var groupName = message.Receiver;
                var members = await _groupService.GetGroupMembers(groupName);

                // Check if the sender is in the group
                if (!members.Contains(username))
                    return BadRequest("You are not in the group " + message.Receiver);

                // Send messages to group members
                await _hubContext.Clients.Group(groupName).ReceiveMessage(message);
            }
            else
            {
                var receiver = message.Receiver;
                if (!await _userService.UserExists(receiver))
                {
                    throw new ArgumentException("User " + receiver + " doesn't exist");
                }
                await _hubContext.Clients.User(receiver).ReceiveMessage(message);
            }

            var messageId = await _messageService.SaveMessage(message);
            if (message.Type == MessageType.UserMessage)
            {
                _logger.LogInformation("User {} sent a message to user {}, message id ", username, message.Content,
                    message.Receiver, messageId);
            }
            else
            {
                _logger.LogInformation("User {} sent a message to group {}, message id ", username, message.Content,
                    message.Receiver, messageId);
            }
            return CreatedAtAction(nameof(SendMessage), new { Id = messageId });
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