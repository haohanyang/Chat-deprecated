using Chat.Common.Dtos;

namespace Chat.Common.DTOs;

public class GroupDto : ContactDto
{
    
    public int Id { get; init; }
    public UserDto Creator { get; init; } = new();
    public List<UserDto> Members { get; init; } = new ();
    public DateTime CreatedAt { get; set; }
}