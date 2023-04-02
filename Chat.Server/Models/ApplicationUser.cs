using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Models;

public class ApplicationUser : IdentityUser
{
    public List<Group> Groups { get; } = new();
    public List<UserMessage> UserMessagesSent { get; } = new();
    public List<UserMessage> UserMessagesReceived { get; } = new();
    public List<GroupMessage> GroupMessagesSent { get; } = new();
}