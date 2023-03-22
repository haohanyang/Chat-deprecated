using System.Security.Claims;
using Chat.Common;
using Chat.Common.Logging;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Models;

public class User
{
    private readonly string _userId;
    private readonly HashSet<string> _groupsJoined = new();
    private readonly HashSet<string> _connections = new();

    public string UserId => _userId;
    public HashSet<string> GroupsJoined => _groupsJoined;
    public HashSet<string> Connections => _connections;

    public User(string username)
    {
        _userId = username;
    }

    public bool IsInGroup(string groupId)
    {
        return _groupsJoined.Contains(groupId);
    }

    public Response JoinGroup(string groupId)
    {
        if (GroupsJoined.Add(groupId))
        {
            return ResponseBuilder.JoinGroupMessage(_userId, groupId);
        }

        return ResponseBuilder.UserAlreadyInGroupMessage(_userId, groupId);
    }

    public Response LeaveGroup(string groupId)
    {
        if (GroupsJoined.Remove(groupId))
        {
            return ResponseBuilder.LeaveGroupMessage(_userId, groupId);
        }

        return ResponseBuilder.UserNotInGroupMessage(_userId, groupId);
    }

    public void AddConnection(string groupId)
    {
        _connections.Add(groupId);
    }

    public void RemoveConnection(string groupId)
    {
        _connections.Remove(groupId);
    }
}

public class UsernameBasedUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        var username = connection.GetHttpContext()?.Request.Query["username"];
        var id = username ?? "default";
        return id!;
    }
}