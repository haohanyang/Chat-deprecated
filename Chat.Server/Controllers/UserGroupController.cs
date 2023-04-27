using System.Security.Claims;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Server.Controllers;

public class UserGroupController : Controller
{
    private readonly ILogger<UserGroupController> _logger;

    private readonly UserGroupService _userGroupService;


    public UserGroupController(UserGroupService userGroupService, ILogger<UserGroupController> logger)
    {
        _userGroupService = userGroupService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("create_group")]
    public async Task<ActionResult> CreateGroup([FromBody] string groupId)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is invalid");

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null)
            {
                _logger.LogError("create_group({}) error:{}", groupId, "Invalid user");
                return Unauthorized("Invalid user");
            }

            await _userGroupService.CreateGroup(groupId);
            _logger.LogInformation("create_group({}) ok", groupId);
            return Ok("ok");
        }
        catch (Exception e)
        {
            _logger.LogError("create_group({}) exception:", e.Message);
            return BadRequest(e.Message);
        }
    }


    [Authorize]
    [HttpPost("join_group")]
    public ActionResult JoinGroup([FromBody] string groupId)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is invalid");

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null)
            {
                _logger.LogError("join_group({}) error:{}", groupId, "Invalid user");
                return Unauthorized("Invalid user");
            }

            _userGroupService.JoinGroup(username, groupId);

            _logger.LogInformation("join_group({}) ok", groupId);
            return Ok("ok");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}