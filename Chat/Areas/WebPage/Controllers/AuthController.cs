using System.Security.Authentication;
using Chat.Areas.Api.Services;
using Chat.Areas.WebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Chat.Common.DTOs;

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
            return View(model);
        }


        model.Error = null;
        try
        {
            var token = await _authenticationService.Login(model.Username, model.Password);
            // Set cookie
            Response.Cookies.Append("chat_access_token", token, new CookieOptions()
            {
                HttpOnly = true,
                Path = "/",
                SameSite = SameSiteMode.Strict
            });

            TempData["loggedinUser"] = model.Username;
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

        if (Request.Cookies["chat_access_token"] != null)
        {
            model.Error = "You are already logged in";
            return View(model);
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult Logout() {
        if (Request.Cookies["chat_access_token"] != null) {
           Response.Cookies.Delete("chat_access_token");
           TempData["RedirectMessage"] = new RedirectMessage { Type = RedirectMessageType.SUCCESS, Message = "You have logged out." };
        } else {
            TempData["RedirectMessage"] = new RedirectMessage { Type = RedirectMessageType.ERROR, Message = "You haven't logged in." };
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
            _logger.LogInformation("Model is invalid");
            return View(model);
        }

        try
        {
            var result = await _authenticationService.Register(model.Username, model.Email, model.Password);
            if (result.Succeeded)
            {
                TempData["RedirectMessage"] = new RedirectMessage { Type = RedirectMessageType.SUCCESS, Message = "The account was successfully created." };
                return RedirectToAction("Index", "Home");
            }
            var errors = result.Errors.Select(e => e.Description).ToList();
            model.Errors = errors;
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