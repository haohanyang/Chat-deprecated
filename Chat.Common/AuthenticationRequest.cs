namespace Chat.Common;
using System.ComponentModel.DataAnnotations;
public class AuthenticationRequest
{
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    
}