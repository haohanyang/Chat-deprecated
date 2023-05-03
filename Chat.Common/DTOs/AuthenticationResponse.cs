namespace Chat.Common.DTOs;

public class AuthenticationResponse
{
    public bool Success { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}