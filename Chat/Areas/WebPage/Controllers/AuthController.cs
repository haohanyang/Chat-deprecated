using System.Security.Authentication;
using Chat.Services.Interface;
using Chat.Areas.WebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Chat.Common.Dto;
using System.Text.Json;
using System.Security.Claims;
using Chat.Common.Http;
using Chat.CrossCutting.Exceptions;
namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IAuthService authService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _authService = authService;
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
            var token = await _authService.Authenticate(new LoginRequest()
            {
                Username = model.Username,
                Password = model.Password
            });

            Response.Cookies.Append("chat_access_token", token, new CookieOptions()
            {
                HttpOnly = true,
                Path = "/",
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            });
            return RedirectToAction("Index", "Home");
        }
        catch (AuthenticationException)
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
                model.CurrentUser = user;
                model.Error = "You are already logged in.";
            }
            catch (UserNotFoundException)
            {
                _logger.LogError("User {} has valid token but not found in the database", username);
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
            Response.StatusCode = BadRequest().StatusCode;
            return View(model);
        }
        try
        {
            var user = await _authService.CreateUser(
                new RegisterRequest()
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                });
            return RedirectToAction("Index", "Home");
        }
        catch (ArgumentException e)
        {
            model.Error = e.Message;
            Response.StatusCode = Unauthorized().StatusCode;
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to Register with unexpected error:{}", e.Message);
            model.Error = "Unexpected error";
            Response.StatusCode = 500;
            return View(model);
        }

    }
}