namespace Chat.Common.Requests;

using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required]
    public string Username { get; init; } = null!;

    [Required] 
    public string Password { get; init; } = null!;

    [Required] 
    public string Email { get; init; } = null!;

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}