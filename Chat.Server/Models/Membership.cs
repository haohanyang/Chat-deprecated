using System.ComponentModel.DataAnnotations.Schema;
namespace Chat.Server.Models;

public class Membership
{
    public string MembershipId { get; set; }
    
    [ForeignKey("MemberId")]
    public string MemberId { get; set; }
    public virtual Member Member { get; set; }
    
    [ForeignKey("GroupID")]
    public string GroupId { get; set; }
    public virtual Group Group { get; set; }
}