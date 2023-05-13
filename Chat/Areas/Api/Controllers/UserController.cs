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
    /// Get the logged-in user
    /// </summary> 
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            var user = await _userService.GetUser(username);
            return Ok(user.ToDto());
        }
        catch (ArgumentException e)
        {
            _logger.LogError("User {} has valid token but does not exist in the database", username);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Unexpected error");
        }
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser([FromRoute(Name = "username")] string username)
    {
        try
        {
            var user = await _userService.GetUser(username);
            return Ok(user.ToDto());
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
    public IActionResult UpdateUser([FromBody] UserDTO user, [FromRoute] string username)
    {
        // TODO: implement
        return Ok("ok");
    }

    [HttpGet]
    public async IAsyncEnumerable<UserDTO> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        foreach (var user in users)
        {
            yield return user.ToDto();
        }
    }

    [Authorize]
    [HttpGet("{username}/groups")]
    public async Task<IActionResult> GetJoinedGroups([FromRoute] string username)
    {
        try
        {
            var groups = await _groupService.GetJoinedGroups(username);
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