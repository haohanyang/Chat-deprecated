using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

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
    private readonly ILogger<ChatHub> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IUserGroupService _userGroupService;

    // Debug only
    public ChatHub(ILogger<ChatHub> logger, IBackgroundTaskQueue taskQueue, IUserGroupService userGroupService)
    {
        _logger = logger;
        _taskQueue = taskQueue;
        _userGroupService = userGroupService;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;

        _userGroupService.AddConnection(username, connectionId);
        var (_, userGroups) = _userGroupService.GetUserGroups(username);
        // Add user to joined group communications
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

    /// <summary>
    /// Make sure the user exists (either in memory in database)
    /// Notify the user if user doesn't exist
    /// </summary>
    /// <param name="username">User's username</param>
    /// <returns>(user exists?, groups the user joined)</returns>
    private async Task<(bool,ISet<string>)> GuardUser(string username)
    {
        var currentUser = Context.UserIdentifier!;
        var (userExists, groups) = _userGroupService.GetUserGroups(username);
        if (!userExists)
            await SendResponse(currentUser,
                new RpcResponse(RpcResponseStatus.Error, "u/" + username + " doesn't exist"));

        return (userExists, groups);
    }

    /// <summary>
    /// Make sure the group exists (either in memory in database).
    /// Notify client if the group doesn't exist
    /// </summary>
    /// <param name="groupId">Group's ID</param>
    /// <returns>(group exists?, group members)</returns>
    private async Task<(bool,ISet<string>)> GuardGroup(string groupId)
    {
        var currentUser = Context.UserIdentifier!;
        var (groupExists, members) = _userGroupService.GetGroupMembers(groupId);

        if (!groupExists)
            await SendResponse(currentUser,
                new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist"));
        return (groupExists, members);
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
            await _taskQueue.QueueBackgroundWorkItemAsync(e => new RemoveMemberTask
            {
                CancellationToken = e,
                GroupId = groupId,
                MemberId = username
            });
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
                await Task.WhenAll(
                    connections.Select(connectionId =>
                        Groups.AddToGroupAsync(connectionId, groupId)));
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

        var (userExists,groups) = await GuardUser(username);
        if (!userExists) return;

        var (groupExists,members) = await GuardGroup(groupId);
        if (!groupExists) return;

        
        if (!members.Contains(username) || !groups.Contains(groupId))
        {
            await SendResponse(username,
                new RpcResponse(RpcResponseStatus.Error, "You are not in g/" + groupId));
            return;
        }

        var message = new Message(username, groupId, DateTime.Now, MessageType.GroupMessage, content);
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
        var (receiverExists, _) = await GuardUser(receiver);
        if (receiverExists)
        {
            var message = new Message(username, receiver, DateTime.Now, MessageType.UserMessage, content);
            await Clients.User(receiver).ReceiveMessage(message);
            await _taskQueue.QueueBackgroundWorkItemAsync(token =>
                new AddUserMessageTask { CancellationToken = token, Message = message }
            );
            await Clients.User(username).RpcResponse(new RpcResponse(RpcResponseStatus.Success, "ok"));
        }
        else
        {
            await Clients.User(username)
                .RpcResponse(new RpcResponse(RpcResponseStatus.Error, "u/" + username + " doesn't exist"));
        }
    }

    private async Task SendResponse(string username, RpcResponse response)
    {
        await Clients.User(username).RpcResponse(response);
    }
}