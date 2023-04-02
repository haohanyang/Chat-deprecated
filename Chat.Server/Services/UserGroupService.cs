using Chat.Common;

namespace Chat.Server.Services;

public interface IUserGroupService
{
    public RpcResponse CreateGroup(string groupId);
    public RpcResponse LeaveGroup(string username, string groupId);
    public RpcResponse JoinGroup(string username, string groupId);
    public (bool, ISet<string>) GetUserGroups(string username);
    public IEnumerable<string> GetUserConnections(string username);
    public (bool, ISet<string>) GetGroupMembers(string username);

    public void AddConnection(string username, string connectionId);
    public void RemoveConnection(string username, string connectionId);
}

public class UserGroupService : IUserGroupService
{
    // group to members
    private static readonly IDictionary<string, ISet<string>> _groups = new Dictionary<string, ISet<string>>();

    // user to groups joined
    private static readonly IDictionary<string, ISet<string>> _users = new Dictionary<string, ISet<string>>();
    private static readonly IDictionary<string, ISet<string>> _connections = new Dictionary<string, ISet<string>>();
    private readonly IDatabaseService _databaseService;

    private readonly ILogger<UserGroupService> _logger;


    public UserGroupService(IDatabaseService databaseService, ILogger<UserGroupService> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public IDictionary<string, ISet<string>> Groups => _groups;
    public IDictionary<string, ISet<string>> Users => _users;
    public IDictionary<string, ISet<string>> Connections => _connections;


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
            {
                lock (members)
                {
                    if (!members.Add(username))
                        // User already in the group
                        response = new RpcResponse(RpcResponseStatus.Error, "You are already in the group");
                }
            }
            else
            {
                // fetch data from database
                var (groupExists, dbMembers) = _databaseService.GetGroupMembers(groupId);
                if (groupExists)
                {
                    _groups.Add(groupId, dbMembers);
                    if (!_groups[groupId].Add(username))
                        // User already in the group
                        response = new RpcResponse(RpcResponseStatus.Error, "You are already in the group");
                }
                else
                {
                    return new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist");
                }
            }


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
                // User's groups info should be in memory after connection 
                // So we don't fetch from database
                return new RpcResponse(RpcResponseStatus.Error, "u/" + username + " doesn't exist");

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
            {
                lock (members)
                {
                    if (!members.Remove(username))
                        // User is not in the group
                        response = new RpcResponse(RpcResponseStatus.Error, "You are not in g/" + groupId);
                }
            }
            else
            {
                // fetch data from database
                var (groupExists, dbMembers) = _databaseService.GetGroupMembers(groupId);
                if (groupExists)
                {
                    _groups.Add(groupId, dbMembers);
                    if (!_groups[groupId].Remove(username))
                        response = new RpcResponse(RpcResponseStatus.Error, "You are not in g/" + groupId);
                }
                else
                {
                    return new RpcResponse(RpcResponseStatus.Error, "g/" + groupId + " doesn't exist");
                }
            }


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

    public (bool, ISet<string>) GetUserGroups(string username)
    {
        lock (_users)
        {
            if (_users.TryGetValue(username, out var groups)) return (true, groups);
            // fetch user's joined groups from database
            var (userExists, dbGroups) = _databaseService.GetUserGroups(username);
            if (userExists) _users.Add(username, dbGroups);

            return (userExists, dbGroups);
        }
    }

    public (bool, ISet<string>) GetGroupMembers(string groupId)
    {
        lock (_groups)
        {
            if (_groups.TryGetValue(groupId, out var members)) return (true, members);
            // fetch group members from database 
            var (groupExists, dbMembers) = _databaseService.GetGroupMembers(groupId);
            if (groupExists) _groups.Add(groupId, dbMembers);
            return (groupExists, dbMembers);
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