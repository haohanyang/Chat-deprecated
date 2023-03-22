using Chat.Common.Logging;
using Chat.Common;

namespace Chat.Server.Models;

public class State
{
    private const int DefaultMaxGroupMember = 10;
    private readonly Dictionary<string, Group> _groups = new();
    private readonly Dictionary<string, User> _users = new();

    public Response AddConnection(string userId, string connectionId)
    {
        lock (_users)
        {
            if (!_users.TryGetValue(userId, out var user))
            {
                // The first time user connects
                user = new User(userId);
                _users.Add(userId, user);
            }

            lock (user.Connections)
            {
                user.AddConnection(connectionId);
            }
        }

        return new Response();
    }

    public Response CreateGroup(string groupId)
    {
        lock (_groups)
        {
            if (!_groups.ContainsKey(groupId))
            {
                _groups.Add(groupId, new Group(groupId, DefaultMaxGroupMember));
                return ResponseBuilder.GroupCreatedMessage(groupId);
            }

            return ResponseBuilder.GroupAlreadyExistsMessage(groupId);
        }
    }

    public Group? GetGroup(string groupId)
    {
        if (_groups.TryGetValue(groupId, out var group))
        {
            return group;
        }

        return null;
    }

    public User? GetUser(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return user;
        }

        return null;
    }

    public Response JoinGroup(string userId, string groupId)
    {
        Response? response = null;
        lock (_groups)
        {
            if (_groups.TryGetValue(groupId, out var group))
            {
                lock (group.Members)
                {
                    var _response = group.AddMember(userId);
                    if (_response.Type != ResponseType.Success)
                    {
                        response = _response;
                    }
                }
            }
            else
            {
                return ResponseBuilder.GroupNotExistsMessage(groupId);
            }

            if (_users.TryGetValue(userId, out var user))
            {
                lock (user.GroupsJoined)
                {
                    var _response = user.JoinGroup(groupId);
                    if (_response.Type != ResponseType.Success)
                    {
                        response = _response;
                    }
                }
            }
            else
            {
                return ResponseBuilder.UserNotExistsMessage(userId);
            }

            return response ?? ResponseBuilder.JoinGroupMessage(userId, groupId);
        }
    }

    public Response LeaveGroup(string userId, string groupId)
    {
        Response? response = null;
        lock (_groups)
        {
            if (_groups.TryGetValue(groupId, out var group))
            {
                lock (group.Members)
                {
                    var _response = group.RemoveMember(userId);
                    if (_response.Type != ResponseType.Success)
                    {
                        response = _response;
                    }
                }
            }
            else
            {
                return ResponseBuilder.GroupNotExistsMessage(groupId);
            }

            if (_users.TryGetValue(userId, out var user))
            {
                lock (user.GroupsJoined)
                {
                    var _response = user.LeaveGroup(groupId);
                    if (_response.Type != ResponseType.Success)
                    {
                        response = _response;
                    }
                }
            }
            else
            {
                return ResponseBuilder.UserNotExistsMessage(userId);
            }
        }

        return response ?? ResponseBuilder.LeaveGroupMessage(userId, groupId);
    }

    public IEnumerable<string> GetConnections(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return user.Connections;
        }

        return Enumerable.Empty<string>();
    }

    public Response RemoveConnection(string userId, string connectionId)
    {
        lock (_users)
        {
            if (!_users.TryGetValue(userId, out var user))
            {
                return ResponseBuilder.UserNotExistsMessage(userId);
            }

            lock (user.Connections)
            {
                user.RemoveConnection(connectionId);
                // if (user.Connections.Count == 0)
                // {
                //     _users.Remove(userId);
                // }
            }

            return new Response();
        }
    }
}