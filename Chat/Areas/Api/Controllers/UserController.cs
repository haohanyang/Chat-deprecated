using System.Linq.Expressions;
using Chat.Areas.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Chat.Common.DTOs;
using System.Security.Claims;

namespace Chat.Areas.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;

    public UserController(IUserService userService, IGroupService groupService, ILogger<UserController> logger)
    {
        _userService = userService;
        _groupService = groupService;
        _logger = logger;
    }

    /// <summary>
    /// Get the user's profile
    /// </summary>
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser([FromRoute(Name = "username")] string username)
    {
        try
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                return NotFound($"User {username} doesn't exist");
            }
            return Ok(user);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get user {} with unexpected error:{}", username, e.Message);
            return StatusCode(500, "Unexpected error");
        }
    }

    [Authorize]
    [HttpPut("{username}")]
    public IActionResult UpdateUser([FromBody] UserDto user, [FromRoute] string username)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [Authorize]
    [HttpGet]
    public async IAsyncEnumerable<UserDto> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        foreach (var user in users)
        {
            yield return user.ToDto();
        }
    }

    /// <summary>
    /// Get all groups that the user has joined
    /// </summary>

    [HttpGet("{username}/groups")]
    public async Task<IActionResult> GetJoinedGroups([FromRoute] string username)
    {
        try
        {
            var groups = await _groupService.GetJoinedGroups(username);
            if (groups == null)
            {
                return NotFound($"User {username} doesn't exist");
            }
            return Ok(groups);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Unexpected error");
        }
    }
}