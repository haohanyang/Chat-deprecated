using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Chat.Common;
using Microsoft.AspNetCore.Connections;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Server.Services;

public interface IUserGroupService
{
    public RpcResponse CreateGroup(string groupId);
    public RpcResponse LeaveGroup(string username, string groupId);
    public RpcResponse JoinGroup(string username, string groupId);
    public ISet<string> GetUserGroups(string username);
    public IEnumerable<string> GetUserConnections(string username);
    public ISet<string> GetGroupMembers(string username);

    public void AddConnection(string username, string connectionId);
    public void RemoveConnection(string username, string connectionId);
}

public class UserGroupService : IUserGroupService
{
    private readonly IDatabaseService _databaseService;

    private readonly ILogger<UserGroupService> _logger;
    // group to members
    private static readonly Dictionary<string, HashSet<string>> _groups = new();

    // user to groups joined
    private static readonly Dictionary<string, HashSet<string>> _users = new();
    private static readonly Dictionary<string, HashSet<string>> _connections = new();


    public UserGroupService(IDatabaseService databaseService, ILogger<UserGroupService> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public Dictionary<string, HashSet<string>> Groups => _groups;
    public Dictionary<string, HashSet<string>> Users => _users;
    public Dictionary<string, HashSet<string>> Connections => _connections;


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

    public IEnumerable<string> GetUserConnections(string username)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(username, out var connections))
                return connections;
        }

        return Enumerable.Empty<string>();
    }

    public ISet<string> GetUserGroups(string username)
    {
        lock (_users)
        {
            if (_users.TryGetValue(username, out var groups))
            {
                return groups;
            }
            // fetch user's joined groups from database
            var dbGroups = _databaseService.GetUserGroups(username);
            _users.Add(username, dbGroups);
            return dbGroups;
        }
    }

    public ISet<string> GetGroupMembers(string groupId)
    {
        lock (_groups)
        {
            if (_groups.TryGetValue(groupId, out var members))
            {
                return members;
            }
            // fetch group members from database 
            var dbMembers = _databaseService.GetGroupMembers(groupId);
            _groups.Add(groupId, dbMembers);
            return dbMembers;
        }
    }

    public void AddConnection(string username, string connectionId)
    {
        // Add connection and user
        lock (_connections)
        {
            if (!_connections.TryGetValue(username, out var connections))
                _connections.Add(username, new HashSet<string>());

            lock (_connections[username])
            {
                _connections[username].Add(connectionId);
            }
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
}