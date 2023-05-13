using System.ComponentModel.DataAnnotations.Schema;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Models;

public class Group
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public User Owner { get; set; } = null!;
    public string OwnerId { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    public IEnumerable<Membership> Memberships { get; set; } = new List<Membership>();
    public IEnumerable<GroupMessage> Messages { get; set; } = new List<GroupMessage>();

    public GroupDTO ToDto()
    {
        return new GroupDTO
        {
            Id = Id,
            ClientId = "g" + Name,
            Name = Name,
            AvatarUrl = AvatarUrl,
            Owner = Owner.ToDto(),
            Members = Memberships.Select(e => e.User.ToDto()).ToList(),
        };
    }
}