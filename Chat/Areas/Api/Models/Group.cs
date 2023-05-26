using System.ComponentModel.DataAnnotations.Schema;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Models;

public class Group
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public User Creator { get; set; } = null!;
    public string CreatorId { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;

    public IEnumerable<Membership> Memberships { get; } = new List<Membership>();
    public IEnumerable<GroupMessage> Messages { get; } = new List<GroupMessage>();

    public GroupDto ToDto()
    {
        return new GroupDto
        {
            Id = Id,
            ClientId = "g" + Name,
            Name = Name,
            Avatar = Avatar,
            Creator = Creator.ToDto(),
            Members = Memberships.Select(e => e.User.ToDto()).ToList(),
        };
    }
}