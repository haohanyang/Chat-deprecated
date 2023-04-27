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
    private readonly MessageService _messageService;
    private readonly UserGroupService _userGroupService;

    public MessageController(IHubContext<ChatHub, IChatClient> hubContext,
        ILogger<MessageController> logger, UserGroupService userGroupService, MessageService messageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _userGroupService = userGroupService;
        _messageService = messageService;
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<ActionResult> SendMessage([FromBody] Message message)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is invalid");

            var username = User.FindFirstValue(ClaimTypes.Name)!;
            if (username != message.Sender) return BadRequest("Invalid message");

            if (message.Type == MessageType.GroupMessage)
            {
                var members = await _userGroupService.GetGroupMembers(message.Receiver);
                // Check if sender is in the group
                if (!members.Select(e => e.UserName).Contains(username))
                    return BadRequest("You are not in the group " + message.Receiver);

                // Send messages to group members
                await _hubContext.Clients.User(message.Receiver).ReceiveMessage(message);
                _logger.LogInformation("User {} sent {} to group {}", message.Sender, message.Content,
                    message.Receiver);
            }
            else
            {
                await _hubContext.Clients.Group(message.Receiver).ReceiveMessage(message);
                _logger.LogInformation("u/{} -> g/{} : {}", message.Sender, message.Receiver, message.Content);
            }

            // Persist messages 
            await _messageService.SaveMessage(message);
            return Ok("ok");
        }
        catch (Exception e)
        {
            _logger.LogError("SendMessage error: {}", e.Message);
            return BadRequest(e.Message);
        }
    }
}