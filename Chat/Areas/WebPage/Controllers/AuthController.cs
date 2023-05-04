using System.Security.Authentication;
using Chat.Areas.Api.Services;
using Chat.Areas.WebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Chat.Common.DTOs;
using System.Text.Json;
using System.Security.Claims;
namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class AuthController : Controller
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginViewModel model)
    {

        if (!ModelState.IsValid)
        {
            // TODO: 
            return View(model);
        }

        model.Error = null;
        try
        {
            var token = await _authenticationService.Login(new AuthenticationRequest { Username = model.Username, Password = model.Password });
            // Set cookie
            Response.Cookies.Append("chat_access_token", token, new CookieOptions()
            {
                HttpOnly = true,
                Path = "/",
                SameSite = SameSiteMode.Strict
            });

            TempData["loggedInUser"] = JsonSerializer.Serialize(new UserDTO { Username = model.Username });
            return RedirectToAction("Index", "Home");
        }

        catch (AuthenticationException e)
        {
            model.Error = "The username or password is incorrect.";
            return View(model);
        }
        catch (Exception e)
        {
            model.Error = "Unexpected error";
            _logger.LogError("Failed to login with unexpected error: {}", e.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginViewModel();
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (username != null)
        {
            model.LoggedInUser = new UserDTO { Username = (string)username };
            model.Error = "You are already logged in.";
        }
        return View(model);
    }

    /// TODO: fix
    [HttpPost]
    public IActionResult Logout()
    {
        if (Request.Cookies["chat_access_token"] != null)
        {
            Response.Cookies.Delete("chat_access_token");
            _logger.LogInformation("DElete cookie");
            TempData["RedirectMessage"] = JsonSerializer.Serialize(new RedirectMessage { Type = RedirectMessageType.SUCCESS, Message = "You have logged out." });
        }
        else
        {
            _logger.LogInformation("No kooci");
            TempData["RedirectMessage"] = JsonSerializer.Serialize(new RedirectMessage { Type = RedirectMessageType.ERROR, Message = "You haven't logged in." });
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authenticationService.Register(
                new AuthenticationRequest
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });
            if (result.Succeeded)
            {
                TempData["RedirectMessage"] = JsonSerializer.Serialize(new RedirectMessage { Type = RedirectMessageType.SUCCESS, Message = "The account was successfully created." });
                return RedirectToAction("Index", "Home");
            }
            var errors = result.Errors.Select(e => e.Description).ToList();
            model.Errors = errors;
            return View(model);
        }
        catch (ArgumentException e)
        {
            model.Errors = new List<string> { e.Message };
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to Register with unexpected error:{}", e.Message);
            model.Errors = new List<string> { "Unexpected error" };
            return View(model);
        }

    }
}