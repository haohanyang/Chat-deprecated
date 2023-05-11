using Chat.Common;
using Chat.Areas.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Chat.Common.DTOs;
using System.Security.Claims;

namespace Chat.Areas.Api.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;


    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> CurrentUser()
    {
        try
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userService.GetUser(username);
            return Ok(user);
        }
        catch (ArgumentException e)
        {
            return BadRequest("Invalid user");
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get current user with unexpected error:{}" + e.Message);
            return BadRequest("Unexpected error");
        }
    }

    [Authorize]
    [HttpGet("all")]
    public async IAsyncEnumerable<UserDTO> AllUsers()
    {
        var users = await _userService.GetAllUsers();
        foreach (var user in users)
        {
            yield return user;
        }
    }
}