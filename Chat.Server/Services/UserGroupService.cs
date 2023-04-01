using System.Collections.Concurrent;
using Chat.Common;

namespace Chat.Server.Services;

public interface IUserGroupService
{
    public RpcResponse CreateGroup(string groupId);
    public RpcResponse LeaveGroup(string username, string groupId);
    public RpcResponse JoinGroup(string username, string groupId);
    public IEnumerable<string>? GetUserGroups(string username);
    public IEnumerable<string>? GetUserConnections(string username);
    public IEnumerable<string>? GetGroupMembers(string username);
    public void AddMessage(Message message);
    public void AddConnection(string username, string connectionId);
    public void RemoveConnection(string username, string connectionId);
}

public class UserGroupService : IUserGroupService
{
    // group to members
    private static readonly Dictionary<string, HashSet<string>> _groups = new();

    // user to groups joined
    private static readonly Dictionary<string, HashSet<string>> _users = new();
    private static readonly Dictionary<string, HashSet<string>> _connections = new();
    private static readonly ConcurrentQueue<Message> _messages = new();

    
    public Dictionary<string, HashSet<string>> Groups => _groups;
    public Dictionary<string, HashSet<string>> Users => _users;
    public Dictionary<string, HashSet<string>> Connections => _connections;
    public ConcurrentQueue<Message> Messages => _messages;

    public RpcResponse CreateGroup(string groupId)
    {
        
        lock (_groups)
        {
            if (_groups.TryGetValue(groupId, out var members))
                // Group already exists
                return new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " already exists");

            _groups.Add(groupId, new HashSet<string>());
            return new RpcResponse(RpcResponseStatus.Success, "ok");
        }
    }

    public RpcResponse JoinGroup(string username, string groupId)
    {
        RpcResponse? response = null;

        lock (_groups)
        lock (_users)
        {
            if (_groups.TryGetValue(groupId, out var members))
                lock (members)
                {
                    if (!members.Add(username))
                        // User already in the group
                        response = new RpcResponse(RpcResponseStatus.Error, "You are already in the group");
                }
            else
                return new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist");


            if (_users.TryGetValue(username, out var groups))
                lock (groups)
                {
                    if (!groups.Add(groupId))
                    {
                        if (response == null)
                        {
                            // Inconsistent
                        }

                        // User already in the group
                        response = new RpcResponse(RpcResponseStatus.Error, "You are already in the group");
                    }
                }
            else
                return new RpcResponse(RpcResponseStatus.Error, "u/" + groupId + " doesn't exist");

            return response ?? new RpcResponse(RpcResponseStatus.Success, "ok");
        }
    }

    public RpcResponse LeaveGroup(string username, string groupId)
    {
        RpcResponse? response = null;

        lock (_groups)
        lock (_users)
        {
            if (_groups.TryGetValue(groupId, out var members))
                lock (members)
                {
                    if (!members.Remove(username))
                        // User is not in the group
                        response = new RpcResponse(RpcResponseStatus.Error, "You are not in g/" + groupId);
                }
            else
                return new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist");


            if (_users.TryGetValue(username, out var groups))
                lock (groups)
                {
                    if (!groups.Remove(groupId))
                    {
                        // User is not in the group
                        if (response == null)
                        {
                            // Inconsistent
                        }

                        response = new RpcResponse(RpcResponseStatus.Error, "You are not in g/" + groupId);
                    }
                }
            else
                return new RpcResponse(RpcResponseStatus.Error, "u/" + username + " doesn't exist");

       
            return response ?? new RpcResponse(RpcResponseStatus.Success, "ok");
        }
    }

    public IEnumerable<string>? GetUserConnections(string username)
    {
        if (_connections.TryGetValue(username, out var connections))
            return connections;
        return null;
    }

    public IEnumerable<string>? GetUserGroups(string username)
    {
        if (_users.TryGetValue(username, out var groups))
            return groups;
        return null;
    }

    public IEnumerable<string>? GetGroupMembers(string groupId)
    {
        if (_groups.TryGetValue(groupId, out var members))
            return members;
        return null;
    }

    public void AddConnection(string username, string connectionId)
    {
        // Add connection and user
        lock (_connections)
        lock (_users)
        {
            if (!_connections.TryGetValue(username, out var connections))
                _connections.Add(username, new HashSet<string>());

            lock (_connections[username])
            {
                _connections[username].Add(connectionId);
            }

            if (!_users.TryGetValue(username, out _)) _users.Add(username, new HashSet<string>());
        }
    }

    public void RemoveConnection(string username, string connectionId)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(username, out var connections))
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                        _connections.Remove(username);
                }
        }
    }

    public void AddMessage(Message message)
    {
        _messages.Enqueue(message);
    }
}