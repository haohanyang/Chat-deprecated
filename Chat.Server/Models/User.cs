using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Models;

public class User : IdentityUser
{
    public IEnumerable<Membership> Memberships { get; set; } = new List<Membership>();
    public IEnumerable<UserMessage> UserMessagesSent { get; set; } = new List<UserMessage>();
    public IEnumerable<UserMessage> UserMessagesReceived { get; set; } = new List<UserMessage>();
    public IEnumerable<GroupMessage> GroupMessagesSent { get; set; } = new List<GroupMessage>();
    
}