using System.Security.Authentication;
using System.Security.Claims;
using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Server.Controllers;

[ApiController]
public class AuthenticationController : Controller
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost("/api/register")]
    public async Task<IActionResult> Register([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid) 
            return BadRequest("Model state is invalid");

        try
        {
            var result = await _authenticationService.Register(request.Username, request.Password);

            if (result.Succeeded)
                return CreatedAtAction(nameof(Register), new { username = request.Username }, request);

            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return BadRequest("Errors:\n"+string.Join("\n", result.Errors.Select(e => e.Description)));
        }
        catch (ArgumentException e)
        {
            // User already exists
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Login {} with unknown error:{}", request.Username, e.Message);
            return BadRequest("Unknown error");
        }
    }

    
    [HttpPost("/api/login")]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid) 
            return BadRequest("Model state is invalid");
        try
        {
            var token = await _authenticationService.Login(request.Username, request.Password);
            return Ok(new AuthenticationResponse
            {
                Username = request.Username,
                Token = token
            });
        }
        catch (AuthenticationException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Login {} with unknown error: {}", request.Username, e.Message);
            return BadRequest("Unknown error");
        }
    }

    [Authorize]
    [HttpGet("/api/auth")]
    public ActionResult<string> CheckAuth()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!;
        return "Hello " + username;
    }
}