using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Server.Models;

public class Membership
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public string MemberId { get; set; }

    [ForeignKey("MemberId")] public ApplicationUser Member { get; set; }

    public string GroupId { get; set; }

    [ForeignKey("GroupId")] public Group Group { get; set; }
}