using Chat.Common.Logging;
using Chat.Common;

namespace Chat.Server.Models;

public class Group
{
    private HashSet<string> _members = new();
    private readonly string _groupId;
    private readonly int _maxMembers;

    public HashSet<string> Members => _members;

    public Group(string groupId, int maxMembers)
    {
        _groupId = groupId;
        _maxMembers = maxMembers;
    }

    public bool HasMember(string userId)
    {
        return _members.Contains(userId);
    }

    public Response AddMember(string userId)
    {
        if (_members.Add(userId))
        {
            return ResponseBuilder.JoinGroupMessage(userId, _groupId);
        }

        return ResponseBuilder.UserAlreadyInGroupMessage(userId, _groupId);
    }

    public Response RemoveMember(string userId)
    {
        if (_members.Remove(userId))
        {
            return ResponseBuilder.LeaveGroupMessage(userId, _groupId);
        }

        return ResponseBuilder.UserNotInGroupMessage(userId, _groupId);
    }
}