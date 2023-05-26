using System.Security.Authentication;
using Chat.Areas.Api.Services;
using Chat.Areas.WebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Chat.Common.DTOs;
using System.Text.Json;
using System.Security.Claims;
using Chat.Common.Requests;

namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginViewModel model)
    {

        if (!ModelState.IsValid)
        {
            Response.StatusCode = BadRequest().StatusCode;
            return View(model);
        }

        model.Error = null;
        try
        {
            var (user, token) = await _userService.Login(new LoginRequest { Username = model.Username, Password = model.Password });
            // Set cookie
            Response.Cookies.Append("chat_access_token", token, new CookieOptions()
            {
                HttpOnly = true,
                Path = "/",
                SameSite = SameSiteMode.Strict
            });

            TempData["CurrentUser"] = JsonSerializer.Serialize(user);
            return RedirectToAction("Index", "Home");
        }
        catch (AuthenticationException e)
        {
            model.Error = "The username or password is incorrect.";
            Response.StatusCode = 401;
            return View(model);
        }
        catch (Exception e)
        {
            model.Error = "Unexpected error";
            Response.StatusCode = 500;
            _logger.LogError("Failed to login with unexpected error: {}", e.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        var model = new LoginViewModel();
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (username != null)
        {
            try
            {
                var user = await _userService.GetUser(username);
                if (user == null)
                {
                    _logger.LogError("User {} has valid token but not found in the database", username);
                }
                model.CurrentUser = user;
                model.Error = "You are already logged in.";
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user {} with unexpected error: {}", username, e.Message);
            }
        }
        return View(model);
    }
    
    [HttpPost]
    public IActionResult Logout()
    {
        if (Request.Cookies["chat_access_token"] != null)
        {
            Response.Cookies.Delete("chat_access_token");
            TempData["RedirectMessage"] = JsonSerializer.Serialize(
                new RedirectMessage { Type = RedirectMessageType.Success, Message = "You have logged out." });
        }
        else
        {
            TempData["RedirectMessage"] = JsonSerializer.Serialize(
                new RedirectMessage { Type = RedirectMessageType.Error, Message = "You haven't logged in." });
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterViewModel();
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (username != null)
        {
            model.CurrentUser = new UserDto { Username = username };
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = BadRequest().StatusCode;
            return View(model);
        }

        try
        {
            var result = await _userService.Register(
                new RegisterRequest()
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });
            if (result.Succeeded)
            {
                TempData["RedirectMessage"] = JsonSerializer.Serialize(
                    new RedirectMessage { Type = RedirectMessageType.Success, Message = "The account was successfully created." });
                return RedirectToAction("Index", "Home");
            }
            var errors = result.Errors.Select(e => e.Description).ToList();
            model.Errors = errors;
            return View(model);
        }
        catch (ArgumentException e)
        {
            model.Errors = new List<string> { e.Message };
            Response.StatusCode = Unauthorized().StatusCode;
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to Register with unexpected error:{}", e.Message);
            model.Errors = new List<string> { "Unexpected error" };
            Response.StatusCode = 500;
            return View(model);
        }

    }
}