using System.ComponentModel.DataAnnotations;

namespace Chat.Areas.WebPage.Models;

public class LoginViewModel : BaseViewModel
{

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? Error { get; set; } = null;

}