using Chat.Common;
using Chat.Common.Dto;
using Chat.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Areas.SignalR;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IConnectionService _connectionService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IUserService _userService;
    private readonly IGroupChannelService _groupChannelService;
    private readonly IUserChannelService _userChannelService;
    // private readonly IUserChannelService _userChannelService;

    public ChatHub(ILogger<ChatHub> logger, IUserService userService, IGroupChannelService groupChannelService, IConnectionService connectionService)
    {
        _logger = logger;
        _userService = userService;
        _groupChannelService = groupChannelService;
        _connectionService = connectionService;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("New connection from {}, connectionId {}", username, connectionId);

        // Add connection
        var connections = _connectionService.AddConnection(username, connectionId);
        _logger.LogInformation("New connection {} from {}, {} connections in total", connectionId, username, connections.Count);

        try
        {
            var groupChannels = await _groupChannelService.GetChannels(username);
            var userChannels = await _userChannelService.GetChannels(username);
            var groupTasks = groupChannels.Select(channel =>
                Groups.AddToGroupAsync(connectionId, channel.Id.ToString()));
            var userTasks = userChannels.Select(channel =>
                Groups.AddToGroupAsync(connectionId, channel.Id.ToString()));
            await Task.WhenAll(groupTasks.Concat(userTasks));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add user {}'s connection {} to groups: {}", username, connectionId, e.Message);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;

        // Remove connection
        var connections = _connectionService.RemoveConnection(username, connectionId);
        _logger.LogInformation("Connection {} disconnected from {}, {} connections remain", connectionId, username, connections.Count);

        try
        {
            var groupChannels = await _groupChannelService.GetChannels(username);
            var userChannels = await _userChannelService.GetChannels(username);
            var groupTasks = groupChannels.Select(channel =>
                Groups.RemoveFromGroupAsync(connectionId, channel.Id.ToString()));
            var userTasks = userChannels.Select(channel =>
                Groups.RemoveFromGroupAsync(connectionId, channel.Id.ToString()));
            await Task.WhenAll(groupTasks.Concat(userTasks));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to remove user {}'s connection {} from groups: {}", username, connectionId,
                e.Message);
        }

        await base.OnDisconnectedAsync(exception);
    }
}