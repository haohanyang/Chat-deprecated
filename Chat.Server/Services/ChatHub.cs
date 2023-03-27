using System.Collections.Concurrent;
using Chat.Common;
using Microsoft.AspNetCore.Authorization;
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

    private static readonly Dictionary<string, HashSet<string>> _groups = new();
    private static readonly Dictionary<string, HashSet<string>> _users = new();
    private static readonly Dictionary<string, HashSet<string>> _connections = new();
    private static readonly ConcurrentQueue<Message> _messages = new();
    
    //private readonly IDatabaseService _databaseService;
    
    // Debug only
    public Dictionary<string, HashSet<string>> GP => _groups;
    public Dictionary<string, HashSet<string>> US => _users;
    public Dictionary<string, HashSet<string>> CN => _connections;
    public ConcurrentQueue<Message> MSG => _messages;
    
    // public ChatHub(IDatabaseService databaseService)
    // {
    //     _databaseService = databaseService;
    // }
    //
    private IEnumerable<string>? GetGroups(string username)
    {
        if (_users.TryGetValue(username, out var user))
        {
            return user;
        }

        return null;
    }

    private IEnumerable<string>? GetMembers(string groupId)
    {
        if (_groups.TryGetValue(groupId, out var group))
        {
            return group;
        }

        return null;
    }

    public IEnumerable<string> GetConnections(string username)
    {
        if (_connections.TryGetValue(username, out var connections))
        {
            return connections;
        }
        return Enumerable.Empty<string>();
    }

    private async Task<IEnumerable<string>?> GuardUser(string username)
    {
        var currentUser = Context.UserIdentifier!;
        var groups = GetGroups(username);
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
        var members = GetMembers(groupId);

        if (members == null)
        {
            await SendResponse(currentUser,
                new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist"));
            return null;
        }

        return members;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;

        // Add connection and user
        lock (_connections) lock (_users)
            {

                if (!_connections.TryGetValue(username, out var connections))
                {
                    connections = new();
                    _connections.Add(username, new HashSet<string>());
                }

                lock (_connections[username])
                {
                    _connections[username].Add(connectionId);
                }


                if (!_users.TryGetValue(username, out _))
                {
                    _users.Add(username, new HashSet<string>());
                }

            }

        // Add user to group communications
        await Task.WhenAll(_users[username].Select(groupId =>
                    Groups.AddToGroupAsync(connectionId, groupId)));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.UserIdentifier!;
        var connectionId = Context.ConnectionId;

        lock (_connections)
        {
            if (_connections.TryGetValue(username, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _connections.Remove(username);
                    }
                }
            }
            // TODO: remove user?
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateGroup(string groupId)
    {
        var username = Context.UserIdentifier!;
        RpcResponse? response;
        lock (_groups)
        {
            if (_groups.TryGetValue(groupId, out var members))
            {
                // Group already exists
                response = new RpcResponse(RpcResponseStatus.Warning, "g/" + groupId + " already exists");
            }
            else
            {
                _groups.Add(groupId, new HashSet<string>());
                response = new RpcResponse(RpcResponseStatus.Success, "g/" + groupId + " is created");
            }
        }

        if (response.Status == RpcResponseStatus.Success)
        {
            //_databaseService.CreateGroup(username, groupId);
        }
        await SendResponse(username, response);
    }

    public async Task LeaveGroup(string groupId)
    {
        var username = Context.UserIdentifier!;
        
        RpcResponse? response = null;
        bool error = false;
        
        lock (_groups) lock(_users)
        {
            if (_groups.TryGetValue(groupId, out var members))
            {
                lock (members)
                {
                    if (!members.Remove(username))
                    {
                        // User is not in the group
                        response = new RpcResponse(RpcResponseStatus.Warning, "You are not in g/" + groupId);
                    }
                }
            }
            else
            {
                response = new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist");
                error = true;
            }
        
            if (!error)
            {
                if (_users.TryGetValue(username, out var groups))
                {
                    lock (groups)
                    {
                        if (!groups.Remove(groupId))
                        {
                            // User is not in the group
                            if (response == null)
                            {
                                // Inconsistent
                            }
                            response = new RpcResponse(RpcResponseStatus.Warning, "You are not in g/" + groupId);
                        }
                    }
                }
                else
                {
                    response = new RpcResponse(RpcResponseStatus.Error, "u/" + username + " doesn't exist");
                    error = true;
                }
            }
        }
        
        if (response == null)
        {
            // No error or warning
            await SendResponse(username, new RpcResponse(RpcResponseStatus.Success, "You have left g/" + groupId));
            // Remove user from collections
            await Task.WhenAll(
                GetConnections(username).Select(connectionId =>
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

        RpcResponse? response = null;
        bool error = false;

        lock (_groups) lock (_users)
            {
                if (_groups.TryGetValue(groupId, out var members))
                {
                    lock (members)
                    {
                        if (!members.Add(username))
                        {
                            // User already in the group
                            response = new RpcResponse(RpcResponseStatus.Warning, "You are already in the group");
                        }
                    }
                }
                else
                {
                    response = new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist");
                    error = true;
                }

                if (!error)
                {
                    if (_users.TryGetValue(username, out var groups))
                    {
                        lock (groups)
                        {
                            if (!groups.Add(groupId))
                            {
                                if (response == null)
                                {
                                    // Inconsistent
                                }
                                // User already in the group
                                response = new RpcResponse(RpcResponseStatus.Warning, "You are already in the group");
                            }
                        }
                    }
                    else
                    {
                        response = new RpcResponse(RpcResponseStatus.Error, "u/" + groupId + " doesn't exist");
                        error = true;
                    }
                }
            }

        if (response == null)
        {
            // No error or warning
            var notification = new Notification(DateTime.Now, "u/" + username + " joined the group");
            await Clients.Group(groupId).ReceiveNotification(notification);
            await Task.WhenAll(
                GetConnections(username).Select(connectionId =>
                    Groups.AddToGroupAsync(connectionId, groupId)));
            //_databaseService.JoinGroup(username, groupId);
            await SendResponse(username, new RpcResponse(RpcResponseStatus.Success, "You have joined g/" + groupId));
        }
        else
        {
            await SendResponse(username, response);
        }
    }

    private async Task SendResponse(string username, RpcResponse response)
    {

        await Clients.User(username).RpcResponse(response);
    }
    public async Task SendUserMessage(string receiver, string content)
    {
        var username = Context.UserIdentifier!;

        if (await GuardUser(receiver) != null)
        {
            var message = new Message(username, receiver, DateTime.Now, ReceiverType.User, content);
            // Add to message queue
            _messages.Enqueue(message);
            
            await Clients.User(receiver).ReceiveMessage(message);
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
        Console.WriteLine(_connections[username].Count);
        var message = new Message(username, groupId, DateTime.Now, ReceiverType.Group, content);
        
        // Add to message queue
        _messages.Enqueue(message);
      
        await Clients.Group(groupId).ReceiveMessage(message);
    }
}
