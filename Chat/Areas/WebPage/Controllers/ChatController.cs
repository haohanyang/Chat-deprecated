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

    public async Task<IActionResult> Index()
    {
        var model = new ChatViewModel();
        try
        {
            var token = Request.Cookies["chat_access_token"];
            if (token != null)
            {
                var result = await _authenticationService.ValidateToken(token);

                if (result.IsValid && result.Claims.TryGetValue(ClaimTypes.NameIdentifier, out var username))
                {
                    model.LoggedInUser = new UserDTO { Username = (string)username };
                }

            }
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError("Index with unexpected error: {}", e.Message);
        }

        return View(model);
    }
}