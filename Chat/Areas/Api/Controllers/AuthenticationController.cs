using System.Security.Authentication;
using System.Security.Claims;
using Chat.Areas.Api.Services;
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IUserService userService, ILogger<AuthenticationController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's username
    /// </summary>
    [HttpGet]
    public ActionResult<string> GetUsername()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (username == null)
        {
            return Unauthorized("You haven't logged in");
        }
        return username;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");
        try
        {
            var result = await _userService.Register(request);

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
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Register user {} failed with unexpected error:{}", request.Username, e.Message);
            return StatusCode(500, "Unexpected error");
        }
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");
        try
        {
            var token = await _userService.Login(request);
            return Ok(new AuthenticationResponse
            {
                Success = true,
                Username = request.Username,
                Token = token
            });
        }
        catch (AuthenticationException e)
        {
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Login {} with unexpected error: {}", request.Username, e.Message);
            return BadRequest("Unexpected error");
        }
    }

    /// <summary>
    /// Validate a token
    /// </summary>
    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery(Name = "token")] string token)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model state is invalid");
        try
        {
            var result = await _userService.ValidateToken(token);
            if (result.IsValid)
            {
                return Ok("Token is valid");
            }
            return Ok("Token is invalid");
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to verify token with unexpected error: {}", e.Message);
            return BadRequest("Unexpected error");
        }

    }
}