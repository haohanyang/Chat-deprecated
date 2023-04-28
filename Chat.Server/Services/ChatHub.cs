using Chat.Common;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Services;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
    Task ReceiveNotification(Notification notification);
    Task RpcResponse(RpcResponse response);
}

// For demo test
public class ChatHub2 : Hub<IChatClient>
{
    private readonly ILogger<ChatHub2> _logger = new LoggerFactory().CreateLogger<ChatHub2>();

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Connection from {}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Disconnection from {}", Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }

    public void Send()
    {
        Clients.All.ReceiveMessage(new Message
        {
            Sender = "sender", Receiver = "receiver", Content = "content", Type = MessageType.UserMessage,
            Time = new DateTime()
        });
    }
}

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

    // Test only, assume sender is valid
    public async Task SendGroupMessage(string sender, string groupName, string content)
    {
        try
        {
            var groups = await _userGroupService.GetJoinedGroups(sender);
            if (!groups.Select(e => e.GroupName).Contains(groupName))
                throw new ArgumentException("You are is not in the group " + groupName);
            await Clients.Groups(groupName).ReceiveMessage(new Message { Sender = sender, Receiver = groupName, Content = content});
        }
        catch (Exception e)
        {
            await Clients.User(sender).RpcResponse(new RpcResponse(RpcResponseStatus.Error, e.Message));
        }
    }
    
    // Test only, assume sender and receivers are valid
    public async Task SendUserMessage(string sender, string receiver, string content)
    {
        try
        {
            await Clients.User(receiver).ReceiveMessage(new Message { Sender = sender, Receiver = receiver, Content = content});
        }
        catch (Exception e)
        {
            await Clients.User(sender).RpcResponse(new RpcResponse(RpcResponseStatus.Error, e.Message));
        }
    }
}