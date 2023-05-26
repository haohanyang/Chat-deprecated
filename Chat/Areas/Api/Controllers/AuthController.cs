using System.Security.Authentication;
using System.Security.Claims;
using Chat.Areas.Api.Services;
using Chat.Common.DTOs;
using Chat.Common.Requests;
using Chat.Common.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetCurrentUser()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (username == null)
        {
            return Unauthorized("You haven't logged in");
        }
        try
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                _logger.LogError("User {} has valid token but not found in the database", username);
                return Unauthorized("You haven't logged in");
            }
            return Ok(user);
        } catch (Exception e)
        {
            _logger.LogError("Failed to get user {} with unexpected error: {}", username, e.Message);
            return StatusCode(500, "Unexpected error");
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new AuthResponse
            {
                Success = false,
                Errors = errors
            });
        }
        try
        {
            var result = await _userService.Register(request);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {} was created", request.Username);
                return CreatedAtAction(nameof(Register), new { username = request.Username }, request);
            }
            
            return BadRequest(new AuthResponse
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
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new AuthResponse
            {
                Success = false,
                Errors = errors
            });
        }
            
        try
        {
            var (user, token) = await _userService.Login(request);
            return Ok(new AuthResponse
            {
                Success = true,
                User = user,
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
}