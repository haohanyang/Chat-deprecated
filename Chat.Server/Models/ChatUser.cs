using System.Security.Claims;
using Chat.Common;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Models;



public class ChatUser
{
    private readonly string _username;
    private readonly HashSet<string> _groups = new();
    private readonly HashSet<string> _connections = new();
    
    public HashSet<string> Groups => _groups;
    public HashSet<string> Connections => _connections;

    public ChatUser(string username)
    {
        _username = username;
    }

    public bool IsInGroup(string groupId)
    {
        return _groups.Contains(groupId);
    }

    public bool JoinGroup(string groupId)
    {
        return _groups.Add(groupId);
    }

    public bool LeaveGroup(string groupId)
    {
        return _groups.Remove(groupId);

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
        return connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    }
}