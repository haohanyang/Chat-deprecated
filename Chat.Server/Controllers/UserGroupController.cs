using System.Security.Claims;
using Chat.Common;
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
    public ActionResult CreateGroup([FromBody] string groupId)
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

            var result = _userGroupService.CreateGroup(groupId);
            if (result.Status == RpcResponseStatus.Success)
            {
                _logger.LogInformation("create_group({}) ok", groupId);
                return Ok("ok");
            }

            return BadRequest(result);
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

            var result = _userGroupService.JoinGroup(username, groupId);
            if (result.Status == RpcResponseStatus.Success)
            {
                _logger.LogInformation("join_group({}) ok", groupId);
                return Ok("ok");
            }

            return BadRequest(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}