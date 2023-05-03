using System.ComponentModel.DataAnnotations;

namespace Chat.Areas.WebPage.Models;

public class LoginViewModel
{

    [Required]
    [MinLength(4)]

    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public string? Error { get; set; } = null;
    public bool LoggedIn { get; set; } = false;

}