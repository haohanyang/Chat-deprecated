using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Areas.Api.Models;


public class Membership
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = new();
}