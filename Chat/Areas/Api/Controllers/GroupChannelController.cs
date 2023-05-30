using System.Security.Claims;
using Chat.Services.Interface;
using Chat.Common;
using Chat.Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chat.Areas.SignalR;
using Chat.Areas.Api.Controllers.Filters;

namespace Chat.Areas.Api.Controllers;

[Route("api/group_channels")]
[ApiController]
[ServiceFilter(typeof(ExceptionFilter))]
public class GroupChannelController : ControllerBase
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<GroupChannelController> _logger;
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;

    private readonly IGroupChannelService _groupChannelService;

    public GroupChannelController(IHubContext<ChatHub, IChatClient> hubContext,
        ILogger<GroupChannelController> logger, IUserService userService, IGroupChannelService groupChannelService, IMessageService messageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _messageService = messageService;
        _userService = userService;
        _groupChannelService = groupChannelService;
    }



    [HttpGet]
    public async IAsyncEnumerable<GroupChannelDto> GetAllChannels([FromRoute(Name = "channel_id")] int id)
    {

        var channels = await _groupChannelService.GetAllChannels();
        foreach (var channel in channels)
        {
            yield return channel;
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<GroupChannelDto>> CreateChannel([FromBody] GroupChannelDto channel)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != channel.Creator.Username)
            throw new ArgumentException("You can only create a channel for yourself");

        var newChannel = await _groupChannelService.CreateChannel(username, channel.Name);
        return CreatedAtAction(nameof(GetChannel), newChannel);
    }


    [HttpGet("{channel_id:int}")]
    public async Task<ActionResult<GroupChannelDto>> GetChannel([FromRoute(Name = "channel_id")] int id)
    {

        var channel = await _groupChannelService.GetChannel(id);
        return Ok(channel);

    }

    [HttpGet("{channel_id:int}/members")]
    public async IAsyncEnumerable<UserDto> GetMembers([FromRoute(Name = "channel_id")] int channelId)
    {
        var memberships = await _groupChannelService.GetMemberships(channelId);
        foreach (var membership in memberships)
        {
            yield return membership.Member;
        }
    }

    /// <summary>
    /// Get all messages in
    /// </summary>
    [Authorize]
    [HttpGet("{channel_id:int}/messages")]
    public async IAsyncEnumerable<MessageDto> GetAllMessages([FromRoute(Name = "channel_id")] int id)
    {
        var messages = await _messageService.GetGroupMessages(id);
        foreach (var message in messages)
        {
            yield return message;
        }
    }

    /// <summary>
    /// Send a message to a group
    /// </summary>
    [Authorize]
    [HttpPost("{channel_id:int}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] MessageDto message, [FromRoute(Name = "channel_id")] int channelId)
    {

        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != message.Author.Username)
            return BadRequest("You can only send messages as yourself");
        if (channelId != message.ChannelId)
            return BadRequest("Invalid channel id");

        // TODO: Check if user is in the group
        await _hubContext.Clients.Group(channelId.ToString()).ReceiveGroupMessage(message);
        var savedMessage = await _messageService.SaveGroupMessage(message);

        _logger.LogInformation("User {} sent a message to channel {}", username, channelId);
        return CreatedAtAction(nameof(SendMessage), savedMessage);
    }
}