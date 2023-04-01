using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Server.Models;

public class Membership
{
    public string MemberId { get; set; }
    public ApplicationUser Member { get; set; }
    public string GroupId { get; set; }
    public Group Group { get; set; }
}