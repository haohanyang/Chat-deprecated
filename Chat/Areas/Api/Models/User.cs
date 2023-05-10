using Microsoft.AspNetCore.Identity;

namespace Chat.Areas.Api.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;
    public IEnumerable<Membership> Memberships { get; set; } = Enumerable.Empty<Membership>();
    public IEnumerable<UserMessage> UserMessagesSent { get; set; } = Enumerable.Empty<UserMessage>();
    public IEnumerable<UserMessage> UserMessagesReceived { get; set; } = Enumerable.Empty<UserMessage>();
    public IEnumerable<GroupMessage> GroupMessagesSent { get; set; } = Enumerable.Empty<GroupMessage>();
}