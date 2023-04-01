using Chat.Common;
using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Services;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
    Task ReceiveNotification(Notification notification);
    Task RpcResponse(RpcResponse response);
}

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<ChatHub> _logger;
    private readonly ApplicationDbContext _applicationDbContext;


    // Debug only
    public ChatHub(ApplicationDbContext applicationDbContext,ILogger<ChatHub> logger,IBackgroundTaskQueue taskQueue,IUserGroupService userGroupService)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _taskQueue = taskQueue;
        _userGroupService = userGroupService;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;

        _userGroupService.AddConnection(username, connectionId);
        var userGroups = _userGroupService.GetUserGroups(username);
        // Add user to joined group communications
        if (userGroups != null)
            await Task.WhenAll(userGroups.Select(groupId =>
                Groups.AddToGroupAsync(connectionId, groupId)));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;

        _userGroupService.RemoveConnection(username, connectionId);
        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task<IEnumerable<string>?> GuardUser(string username)
    {
        var currentUser = Context.UserIdentifier!;
        var groups = _userGroupService.GetUserGroups(username);
        if (groups == null)
        {
            await SendResponse(currentUser,
                new RpcResponse(RpcResponseStatus.Error, "u/" + username + " doesn't exist"));
            return null;
        }

        return groups;
    }
    
    private async Task<IEnumerable<string>?> GuardGroup(string groupId)
    {
        var currentUser = Context.UserIdentifier!;
        var members = _userGroupService.GetGroupMembers(groupId);

        if (members == null)
        {
            await SendResponse(currentUser,
                new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist"));
            return null;
        }

        return members;
    }

    public async Task CreateGroup(string groupId)
    {
        var username = Context.UserIdentifier!;
        var response = _userGroupService.CreateGroup(groupId);
        await SendResponse(username, response);
        await _taskQueue.QueueBackgroundWorkItemAsync(token => new AddGroupTask
        {
            GroupId = groupId
        });
    }

    public async Task LeaveGroup(string groupId)
    {
        var username = Context.UserIdentifier!;

        var response = _userGroupService.LeaveGroup(username, groupId);

        if (response.Status == RpcResponseStatus.Success)
        {
            // No error or warning
            await SendResponse(username, new RpcResponse(RpcResponseStatus.Success, "You have left g/" + groupId));
            // Remove user from collections
            var connections = _userGroupService.GetUserConnections(username);
            if (connections != null)
                await Task.WhenAll(
                    connections.Select(connectionId =>
                        Groups.RemoveFromGroupAsync(connectionId, groupId)));

            var notification = new Notification(DateTime.Now, "u/" + username + " left the group");
            await Clients.Group(groupId).ReceiveNotification(notification);
        }
        else
        {
            await SendResponse(username, response);
        }
    }

    public async Task JoinGroup(string groupId)
    {
        var username = Context.UserIdentifier!;
        var response = _userGroupService.JoinGroup(username, groupId);
        if (response.Status == RpcResponseStatus.Success)
        {
            // Add to ask queue
            // No error or warning
            var notification = new Notification(DateTime.Now, "u/" + username + " joined the group");
            await Clients.Group(groupId).ReceiveNotification(notification);
            var connections = _userGroupService.GetUserConnections(username);
            if (connections != null)
            {
                await Task.WhenAll(
                    connections.Select(connectionId =>
                        Groups.AddToGroupAsync(connectionId, groupId)));
            }
            await SendResponse(username, new RpcResponse(RpcResponseStatus.Success, "You have joined g/" + groupId));
            await _taskQueue.QueueBackgroundWorkItemAsync(token => new AddMemberTask
            {
                GroupId = groupId,
                MemberId = username
            });
        }
        else
        {
            await SendResponse(username, response);
        }
    }
    
    public async Task SendGroupMessage(string groupId, string content)
    {
        var username = Context.UserIdentifier!;

        var groups = await GuardUser(username);
        if (groups == null)
        {
            return;
        }

        var members = await GuardGroup(groupId);
        if (members == null)
        {
            return;
        }

        if (!members.Contains(username) || !groups.Contains(groupId))
        {
            await SendResponse(username, new RpcResponse(RpcResponseStatus.Error, "u/" + username + " is not in g/" + groupId));
            return;
        }
        var message = new Message(username, groupId, DateTime.Now, MessageType.GroupMessage, content);
        // Add to message queue
        _userGroupService.AddMessage(message);
        await Clients.Group(groupId).ReceiveMessage(message);
        await _taskQueue.QueueBackgroundWorkItemAsync(e => new AddGroupMessageTask
        {
            CancellationToken = e,
            Message = message
        });
    }
    
    public async Task SendUserMessage(string receiver, string content)
    {
        var username = Context.UserIdentifier!;
        
        if (await GuardUser(receiver) != null)
        {
            
            var message = new Message(username, receiver, DateTime.Now, MessageType.UserMessage, content);
            _userGroupService.AddMessage(message);
            
            await Clients.User(receiver).ReceiveMessage(message);
            await _taskQueue.QueueBackgroundWorkItemAsync(token =>
                    new AddUserMessageTask { CancellationToken = token, Message = message}
            );
            await Clients.User(username).RpcResponse(new RpcResponse(RpcResponseStatus.Success, "ok"));
        }
    }
    
    private async Task SendResponse(string username, RpcResponse response)
    {
        await Clients.User(username).RpcResponse(response);
    }
}