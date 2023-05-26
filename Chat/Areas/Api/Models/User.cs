using Chat.Common.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Chat.Areas.Api.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Avatar { get; set; } = string.Empty;
    public IEnumerable<Membership> Memberships { get; } = new List<Membership>();
    public IEnumerable<UserMessage> UserMessagesSent { get; } = new List<UserMessage>();
    public IEnumerable<UserMessage> UserMessagesReceived { get; } = new List<UserMessage>();
    public IEnumerable<GroupMessage> GroupMessagesSent { get; } = new List<GroupMessage>();
    public IEnumerable<Group> CreatedGroups { get; } = new List<Group>();
    public UserDto ToDto()
    {
        return new UserDto
        {
            ClientId = "u" + UserName,
            Id = Id,
            Username = UserName!,
            Name = $"{FirstName} {LastName}",
            Avatar = Avatar
        };
    }
}