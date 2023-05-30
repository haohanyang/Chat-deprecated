using Chat.Common.Dto;

namespace Chat.Common.Http;

public class AuthResponse
{
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public string? Error { get; set; }
}