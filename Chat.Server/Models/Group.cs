using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Server.Models;

public class Group
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string GroupName { get; set; }

    public IEnumerable<Membership> Memberships { get; set; } = new List<Membership>();
    public List<GroupMessage> Messages { get; set; } = new();
}