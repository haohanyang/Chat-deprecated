using Microsoft.AspNetCore.Mvc;
using Chat.Areas.WebPage.Models;
using Chat.Areas.Api.Services;
using System.Security.Claims;
using Chat.Common.DTOs;
namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class ChatController : Controller
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public ChatController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new ChatViewModel();
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue(ClaimTypes.Name);
        if (username != null && name != null)
        {
            var names = name.Split(';');
            model.LoggedInUser = new UserDTO
            {
                Username = username,
                FirstName = names[0],
                LastName = names[1]
            };
        }
        return View(model);
    }
}