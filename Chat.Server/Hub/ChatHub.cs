using Chat.Common.Logging;
using Chat.Common;
using Chat.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Hub;

public class ChatHub : Hub<IChatClient>
{
    private readonly static State state = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        state.AddConnection(userId!, connectionId);

        await Task.WhenAll(state.GetUser(userId).GroupsJoined.Select(groupId =>
            Groups.AddToGroupAsync(connectionId, groupId)));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        state.RemoveConnection(userId!, connectionId);
        await base.OnDisconnectedAsync(exception);
    }


    public async Task BroadcastMessage(Notification notification)
    {
        await Clients.All.ReceiveNotification(notification);
    }

    public async Task LeaveGroup(string groupId)
    {
        var userId = Context.UserIdentifier;

        var response = state.LeaveGroup(userId!, groupId);

        if (response.Type == ResponseType.Success)
        {
            await Clients.User(userId!).RpcSucceeds(response.ClientMessage);
            await Task.WhenAll(
                state.GetConnections(userId!).Select(connectionId =>
                    Groups.RemoveFromGroupAsync(connectionId, groupId)));
            var notification = new Notification(DateTime.Now, response.ServerMessage);
            await Clients.Group(groupId).ReceiveNotification(notification);
        }
        else
        {
            await SendResponse(userId!, response);
        }
    }

    public async Task JoinGroup(string groupId)
    {
        var userId = Context.UserIdentifier;
        var response = state.JoinGroup(userId!, groupId);

        if (response.Type == ResponseType.Success)
        {
            var notification = new Notification(DateTime.Now, response.ServerMessage);
            await Clients.Group(groupId).ReceiveNotification(notification);
            await Task.WhenAll(
                state.GetConnections(userId!).Select(connectionId =>
                    Groups.AddToGroupAsync(connectionId, groupId)));
            await Clients.User(userId!).RpcSucceeds(response.ClientMessage);
        }
        else
        {
            await SendResponse(userId!, response);
        }
    }

    private async Task SendResponse(string userId, Response response)
    {
        if (response.Type == ResponseType.Success)
        {
            await Clients.User(userId).RpcSucceeds(response.ClientMessage);
        }
        else if (response.Type == ResponseType.Warning)
        {
            await Clients.User(userId).RpcWarns(response.ClientMessage);
        }
        else
        {
            await Clients.User(userId).RpcFails(response.ClientMessage);
        }
    }

    public async Task CreateGroup(string groupId)
    {
        var userId = Context.UserIdentifier;
        var response = state.CreateGroup(groupId);
        await SendResponse(userId!, response);
    }

    public async Task SendUserMessage(string userId, string message)
    {
        var sender = Context.UserIdentifier;

        if (state.GetUser(userId) is null)
        {
            await SendResponse(sender!, ResponseBuilder.UserNotExistsMessage(userId));
        }

        var m = new Message(sender!, userId, DateTime.Now, ReceiverType.User, message);
        await Clients.User(userId).ReceiveUserMessage(m);
    }


    public async Task SendGroupMessage(string groupId, string message)
    {
        var sender = Context.UserIdentifier;

        var user = state.GetUser(sender!);
        var group = state.GetGroup(groupId);

        if (group is null)
        {
            await SendResponse(sender!, ResponseBuilder.GroupNotExistsMessage(groupId));
            return;
        }

        if (user is null)
        {
            return;
        }

        if (!group.HasMember(sender!) || !user.IsInGroup(groupId))
        {
            await SendResponse(sender!, ResponseBuilder.UserNotInGroupMessage(sender!, groupId, ResponseType.Error));
            return;
        }

        var m = new Message(sender!, groupId, DateTime.Now, ReceiverType.Group, message);
        await Clients.Group(groupId).ReceiveGroupMessage(m);
    }

    public async Task<string> WaitForMessage(string connectionId)
    {
        var message = await Clients.Client(connectionId).GetMessage();
        Console.WriteLine("message:{0}", message);
        return message;
    }
}