using System.Security.Claims;
using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Controllers;

[ApiController]
public class MessageController : Controller
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageService _messageService;
    private readonly IUserGroupService _userGroupService;

    public MessageController(IHubContext<ChatHub, IChatClient> hubContext,
        ILogger<MessageController> logger, IUserGroupService userGroupService, IMessageService messageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _userGroupService = userGroupService;
        _messageService = messageService;
    }

    [Authorize]
    [HttpPost("/api/send")]
    public async Task<ActionResult> SendMessage([FromBody] Message message)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            if (message.Type == MessageType.GroupMessage)
            {
                var groupName = message.Receiver;
                var members = await _userGroupService.GetGroupMembers(groupName);
                
                // Check if the sender is in the group
                if (!members.Contains(username))
                    return BadRequest("You are not in the group " + message.Receiver);

                // Send messages to group members
                await _hubContext.Clients.Group(groupName).ReceiveMessage(message);
            }
            else
            {
                var receiver = message.Receiver;
                if (!await _userGroupService.UserExists(receiver))
                {
                    throw new ArgumentException("User " + receiver + " doesn't exist");
                }
                await _hubContext.Clients.User(receiver).ReceiveMessage(message);
                _logger.LogInformation("User {} sent \"{}\" to user {}", username, message.Content,
                    receiver);
            }

            // Persist messages 
            await _messageService.SaveMessage(message);
            return CreatedAtAction(nameof(SendMessage), new { } , message);
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