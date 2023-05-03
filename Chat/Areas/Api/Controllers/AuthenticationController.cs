using System.Security.Authentication;
using System.Security.Claims;
using Chat.Areas.Api.Services;
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{

    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public string Index()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!;
        return "Hello " + username;
    }

    [HttpPost("register", Name = "Register")]
    public async Task<IActionResult> Register([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");
        try
        {
            var result = await _authenticationService.Register(request.Username, request.Email, request.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {} was created", request.Username);
                return CreatedAtAction(nameof(Register), new { username = request.Username }, request);

            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return BadRequest(new AuthenticationResponse
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            });
        }
        catch (ArgumentException e)
        {
            // User already exists
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Login {} with unknown error:{}", request.Username, e.Message);
            return BadRequest("Unexpected error");
        }
    }

    [HttpPost("login", Name = "Login")]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");
        try
        {
            var token = await _authenticationService.Login(request.Username, request.Password);
            return Ok(new AuthenticationResponse
            {
                Success = true,
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
            return BadRequest("Unexpected error");
        }
    }

    [HttpGet("validate", Name = "Validate")]
    public async Task<IActionResult> Validate([FromQuery(Name = "token")] string token)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");
        try
        {
            var result = await _authenticationService.ValidateToken(token);
            if (result.IsValid)
            {
                return Ok("Token is valid");
            }
            return Ok("Token is invalid");
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to verify token with unknown error: {}", e.Message);
            return BadRequest("Unexpected error");
        }

    }
}