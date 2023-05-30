using System.Security.Authentication;
using System.Security.Claims;
using Chat.Services.Interface;
using Chat.Common.Dto;
using Chat.Common.Http;
using Chat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Chat.Areas.Api.Controllers.Filters;

namespace Chat.Areas.Api.Controllers;

[Route("api/auth")]
[ApiController]
[ServiceFilter(typeof(ExceptionFilter))]
public class AuthController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
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
            return Ok(user);
        }
        catch (UserNotFoundException)
        {
            _logger.LogError("User {} has valid token but not found in the database", username);
            return Unauthorized("You haven't logged in");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            throw new ValidationException(ModelState);
        }
        var user = await _authService.CreateUser(request);
        return CreatedAtAction(nameof(GetCurrentUser), user);
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            throw new ValidationException(ModelState);
        }
        var response = await _authService.Authenticate(request);
        return Ok(response);
    }
}