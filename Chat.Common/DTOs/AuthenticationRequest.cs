using System.ComponentModel.DataAnnotations;

namespace Chat.Common.DTOs;

public class AuthenticationRequest
{
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;

}