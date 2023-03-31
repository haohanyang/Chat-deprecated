using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Models;

public class Group
{
    public string Id { get; set; }
    public List<ApplicationUser> Members { get; } = new();
    public List<GroupMessage> Messages { get; } = new();
}