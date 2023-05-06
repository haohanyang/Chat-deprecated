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
            await Task.WhenAll(groups.Select(groupName =>
                Groups.AddToGroupAsync(connectionId, groupName)));
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
            await Task.WhenAll(groups.Select(groupName =>
                Groups.RemoveFromGroupAsync(connectionId, groupName)));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to remove user {}'s connection {} from groups: {}", username, connectionId,
                e.Message);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Test only, assume sender is valid
    public async Task SendGroupMessage(string sender, string groupName, string content)
    {
        var groups = await _groupService.GetJoinedGroups(sender);
        if (!groups.Contains(groupName))
            throw new ArgumentException("You are not in the group " + groupName);
        await Clients.Group(groupName).ReceiveMessage(new MessageDTO
        { Sender = sender, Receiver = groupName, Content = content });
    }

    // Test only, assume sender and receivers are valid
    public async Task SendUserMessage(string sender, string receiver, string content)
    {
        await Clients.User(receiver)
            .ReceiveMessage(new MessageDTO { Sender = sender, Receiver = receiver, Content = content });
    }
}