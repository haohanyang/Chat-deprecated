using Chat.Common.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Chat.Areas.Api.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;
    public IEnumerable<Membership> Memberships { get; set; } = new List<Membership>();
    public IEnumerable<UserMessage> UserMessagesSent { get; set; } = new List<UserMessage>();
    public IEnumerable<UserMessage> UserMessagesReceived { get; set; } = new List<UserMessage>();
    public IEnumerable<GroupMessage> GroupMessagesSent { get; set; } = new List<GroupMessage>();
    public IEnumerable<Group> OwnedGroups { get; set; } = new List<Group>();
    public UserDTO ToDTO()
    {
        return new()
        {
            Id = Id,
            Username = UserName!,
            Name = $"{FirstName} {LastName}",
            AvararUrl = AvatarUrl
        };
    }
}