using System.Security.Claims;
using Chat.Services.Interface;
using Chat.Common;
using Chat.Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chat.Areas.SignalR;
using Chat.Areas.Api.Controllers.Filters;
using System.Collections.Generic;

namespace Chat.Areas.Api.Controllers;

[Route("api/user_channels")]
[ApiController]
[ServiceFilter(typeof(ExceptionFilter))]
public class UserChannelController : ControllerBase
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<UserChannelController> _logger;
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;
    private readonly IUserChannelService _userChannelService;

    public ChatController(IHubContext<ChatHub, IChatClient> hubContext,
        ILogger<UserChannelController> logger, IUserService userService, IUserChannelService userChannelService, IMessageService messageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _messageService = messageService;
        _userService = userService;
        _userChannelService = userChannelService;
    }

    /// <summary>
    /// Get user channel
    /// </summary>
    [Authorize]
    [HttpGet("{channel_id:int}")]
    public async Task<ActionResult<UserChannelDto>> GetChannel([FromRoute(Name = "channel_id")] int channelId)
    {
        var channel = await _userChannelService.GetChannel(channelId);
        return channel;
    }

    /// <summary>
    /// Get all messages of the user channel
    /// </summary>
    [Authorize]
    [HttpGet("{channel_id:int}/messages")]
    public async IAsyncEnumerable<MessageDto> GetGroupChat([FromRoute(Name = "channel_id")] int channelId)
    {
        var messages = await _messageService.GetGroupMessages(channelId);
        foreach (var message in messages)
        {
            yield return message;
        }
    }

    /// <summary>
    /// Send a message to another user
    /// </summary>
    [Authorize]
    [HttpPost("{channel_id:int}/messages")]
    public async Task<IActionResult> SendUserMessage([FromBody] MessageDto message)
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
    [HttpPost("groups")]
    public async Task<ActionResult> SendGroupMessage([FromBody] GroupMessageDto message)
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