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

    public UserChannelController(IHubContext<ChatHub, IChatClient> hubContext,
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

    [HttpGet("{channel_id:int}")]
    public async Task<ActionResult<UserChannelDto>> GetChannel([FromRoute(Name = "channel_id")] int channelId)
    {
        var channel = await _userChannelService.GetChannel(channelId);
        return channel;
    }

    [HttpGet("{channel_id:int}/members")]
    public async IAsyncEnumerable<UserDto> GetMembers([FromRoute(Name = "channel_id")] int channelId)
    {
        var memberships = await _userChannelService.GetMemberships(channelId);
        foreach (var membership in memberships)
        {
            yield return membership.Member;
        }
    }

    /// <summary>
    /// Get all messages of the user channel
    /// </summary>
    [Authorize]
    [HttpGet("{channel_id:int}/messages")]
    public async IAsyncEnumerable<MessageDto> GetMessages([FromRoute(Name = "channel_id")] int channelId)
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
    public async Task<ActionResult<MessageDto>> SendUserMessage([FromRoute(Name = "channel_id")] int channelId, [FromBody] MessageDto message)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != message.Author.Username)
            throw new ArgumentException("You can only send messages as yourself");
        if (channelId != message.ChannelId)
            throw new ArgumentException("Invalid channel id");


        // TODO: check if the user is in the channel
        var savedMessage = await _messageService.SaveUserMessage(message);
        await _hubContext.Clients.Group(channelId.ToString()).ReceiveUserMessage(savedMessage);

        _logger.LogInformation("User {} sent a message to user channel {}", username,
                        channelId);
        return CreatedAtAction(nameof(SendUserMessage), savedMessage);
    }
}