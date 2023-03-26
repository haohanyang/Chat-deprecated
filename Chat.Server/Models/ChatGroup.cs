using Chat.Common;

namespace Chat.Server.Models;

public class ChatGroup
{
    private HashSet<string> _members = new();
    private readonly string _groupId;

    public HashSet<string> Members => _members;

    public ChatGroup(string groupId)
    {
        _groupId = groupId;
    }

    public bool HasMember(string userId)
    {
        return _members.Contains(userId);
    }

    public bool AddMember(string userId)
    {
        return _members.Add(userId);
    }

    public bool RemoveMember(string userId)
    {
        return _members.Remove(userId);
    }
}