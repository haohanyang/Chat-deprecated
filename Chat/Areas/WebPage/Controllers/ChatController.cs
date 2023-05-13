using Microsoft.AspNetCore.Mvc;
using Chat.Areas.WebPage.Models;
using Chat.Areas.Api.Services;
using System.Security.Claims;
using Chat.Common.DTOs;
namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class ChatController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public ChatController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ChatViewModel();
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (username != null)
        {
            try
            {
                var user = await _userService.GetUser(username);
                model.LoggedInUser = user.ToDto();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get current user with unexpected error:{}" + e.Message);
            }
        }
        return View(model);
    }
}