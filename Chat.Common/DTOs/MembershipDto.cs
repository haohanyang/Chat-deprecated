using Chat.Common.DTOs;

namespace Chat.Common.Dtos;

public class MembershipDto
{
    public int Id { get; set; }
    public UserDto Member { get; set; } = new();
    public GroupDto Group { get; set; } = new();
}