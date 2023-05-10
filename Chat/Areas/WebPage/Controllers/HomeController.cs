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
    private readonly IAuthenticationService _authenticationService;

    public HomeController(ILogger<HomeController> logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    public IActionResult Index()
    {
        var model = new HomeViewModel();


        if (TempData["RedirectMessage"] != null)
        {
            model.RedirectMessage = JsonSerializer.Deserialize<RedirectMessage>((string)TempData["RedirectMessage"]!);
        }

        // If redirected from login
        if (TempData["loggedInUser"] != null)
        {
            model.LoggedInUser = JsonSerializer.Deserialize<UserDTO>((string)TempData["loggedInUser"]!);
        }

        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue(ClaimTypes.Name);
        if (username != null && name != null)
        {
            var names = name.Split(';');
            model.LoggedInUser = new UserDTO
            {
                Username = username,
                FirstName = names[0],
                LastName = names[1],
                // TODO: retrieve avatar url from db
                AvararUrl = "https://api.dicebear.com/6.x/initials/svg?seed=" + names[0][0] + names[1][0]
            };
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}