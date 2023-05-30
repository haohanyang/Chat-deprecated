using Chat.Areas.WebPage.Models;
using Chat.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class ChatController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;

    public ChatController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new ChatViewModel();
        return View("AuthView");
    }
}