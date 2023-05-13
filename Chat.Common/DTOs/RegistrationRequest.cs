using System.ComponentModel.DataAnnotations;

namespace Chat.Common.DTOs;

public class RegistrationRequest
{
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

}