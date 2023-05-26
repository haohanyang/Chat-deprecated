using Chat.Common.DTOs;

namespace Chat.Common.Responses;

public class AuthResponse
{
    public bool Success { get; init; }
    public UserDto? User { get; set; }
    public string? Token { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}