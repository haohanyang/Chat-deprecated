using System.Diagnostics;
using System.Security.Claims;
using Chat.Areas.WebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Chat.Areas.Api.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chat.Common.DTOs;

namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel();
        if (TempData["RedirectMessage"] != null)
        {
            model.RedirectMessage = JsonSerializer.Deserialize<RedirectMessage>((string)TempData["RedirectMessage"]!);
        }

        if (TempData["loggedInUser"] != null)
        {
            model.LoggedInUser = JsonSerializer.Deserialize<UserDTO>((string)TempData["loggedInUser"]!);
        }

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
                model.Error = "Unexpected error";
            }
        }
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}