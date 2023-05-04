using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Areas.Api.Models;


public class Membership
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string UserId { get; set; }
    public string Username {get;set;}
    public User User { get; set; }

    public int GroupId { get; set; }
    public string GroupName {get;set;}
    public Group Group { get; set; }

    public DateTime JoinedTime { get; set; }
}