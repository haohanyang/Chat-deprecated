using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Areas.Api.Models;

public class Group
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;

    public User Owner { get; set; } = null!;
    public string OwnerId { get; set; } = string.Empty;

    public IEnumerable<Membership> Memberships { get; set; } = new List<Membership>();
    public IEnumerable<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}