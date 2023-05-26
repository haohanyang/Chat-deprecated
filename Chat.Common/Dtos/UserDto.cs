using Chat.Common.Dtos;

namespace Chat.Common.DTOs;

public class UserDto : ContactDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = false;
}