using Chat.Common;
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Areas.Api.Services;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IConnectionService _connectionService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;

    public ChatHub(ILogger<ChatHub> logger, IUserService userService, IGroupService groupService, IConnectionService connectionService)
    {
        _logger = logger;
        _userService = userService;
        _groupService = groupService;
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
            // Add user to group communications
            var groups = await _groupService.GetJoinedGroups(username);
            await Task.WhenAll(groups.Select(group =>
                Groups.AddToGroupAsync(connectionId, group.Name)));
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
            // Remove user from connections to groups
            var groups = await _groupService.GetJoinedGroups(username);
            await Task.WhenAll(groups.Select(group =>
                Groups.RemoveFromGroupAsync(connectionId, group.Name)));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to remove user {}'s connection {} from groups: {}", username, connectionId,
                e.Message);
        }

        await base.OnDisconnectedAsync(exception);
    }
}