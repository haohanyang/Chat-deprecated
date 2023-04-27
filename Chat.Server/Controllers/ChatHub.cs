using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Controllers;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
    Task ReceiveNotification(Notification notification);
    Task RpcResponse(RpcResponse response);
}

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IConnectionService _connectionService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IUserGroupService _userGroupService;

    public ChatHub(ILogger<ChatHub> logger, IUserGroupService userGroupService, IConnectionService connectionService)
    {
        _logger = logger;
        _userGroupService = userGroupService;
        _connectionService = connectionService;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("New connection from {}, connectionId {}", username, connectionId);

        // Add connection
        _connectionService.AddConnection(username, connectionId);
        try
        {
            // Add user to group communications
            var groups = await _userGroupService.GetJoinedGroups(username);
            await Task.WhenAll(groups.Select(group =>
                Groups.AddToGroupAsync(connectionId, group.GroupName)));
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
        _connectionService.RemoveConnection(username, connectionId);
        try
        {
            // Remove user from connections to groups
            var groups = await _userGroupService.GetJoinedGroups(username);
            await Task.WhenAll(groups.Select(group =>
                Groups.RemoveFromGroupAsync(connectionId, group.GroupName)));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to remove user {}'s connection {} from groups: {}", username, connectionId,
                e.Message);
        }

        await base.OnDisconnectedAsync(exception);
    }
}