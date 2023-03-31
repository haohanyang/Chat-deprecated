using System.Security.Claims;
using Chat.Common;
using Chat.Server.Models;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Server.Controllers;

[ApiController]
public class AuthenticationController : Controller
{
    private readonly IAuthenticationTokenService _authenticationTokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationController(UserManager<ApplicationUser> userManager,
        IAuthenticationTokenService authenticationTokenService)
    {
        _userManager = userManager;
        _authenticationTokenService = authenticationTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest("Model state is invalid");

        var result = await _userManager.CreateAsync(
            new ApplicationUser { UserName = request.Username }, request.Password);

        if (result.Succeeded)
            return CreatedAtAction(nameof(Register), new { username = request.Username }, request);

        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(string.Join("\n", result.Errors.Select(e => e.Description)));
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");

        var managedUser = await _userManager.FindByNameAsync(request.Username);
        if (managedUser is null)
            return BadRequest("User " + request.Username + " doesn't exist");

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
            return BadRequest("Password is incorrect");

        var accessToken = _authenticationTokenService.GenerateToken(managedUser);
        if (accessToken == null) return BadRequest("Token generation failed");

        return Ok(new AuthenticationResponse
        {
            Username = request.Username,
            Token = accessToken
        });
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> CheckAuth1()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return "Hello " + username;
    }

    [HttpGet("noauth")]
    public ActionResult<string> CheckAuth2()
    {
        return "You are not authenticated";
    }
}