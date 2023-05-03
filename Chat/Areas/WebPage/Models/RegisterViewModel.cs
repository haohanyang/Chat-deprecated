using System.ComponentModel.DataAnnotations;

namespace Chat.Areas.WebPage.Models;

public class RegisterViewModel : BaseViewModel
{


    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = new();
}