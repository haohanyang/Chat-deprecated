using System.ComponentModel.DataAnnotations.Schema;
using Chat.Common.Dto;

namespace Chat.Domain;


public class GroupChannelMembership
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string MemberId { get; set; } = string.Empty;
    public User Member { get; set; } = null!;

    public int ChannelId { get; set; }
    public GroupChannel Channel { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}