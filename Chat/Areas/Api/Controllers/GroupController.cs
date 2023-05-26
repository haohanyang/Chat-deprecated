using System.Security.Claims;
using Chat.Common;
using Chat.Areas.Api.Services;
using Chat.Common.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Controllers;

[Route("api/groups")]
[ApiController]
public class GroupController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly IGroupService _groupService;
    private readonly IConnectionService _connectionService;


    public GroupController(IGroupService groupService, IHubContext<ChatHub, IChatClient> hubContext, IConnectionService connectionService, ILogger<UserController> logger)
    {
        _groupService = groupService;
        _hubContext = hubContext;
        _logger = logger;
        _connectionService = connectionService;
    }

    /// <summary>
    /// Get all groups
    /// </summary>
    [Authorize]
    [HttpGet]
    public async IAsyncEnumerable<GroupDto> GetAllGroups()
    {
        var groups = await _groupService.GetAllGroups();
        foreach (var group in groups)
        {
            yield return group;
        }
    }

    /// <summary>
    /// Get a group by id
    /// </summary>
    [HttpGet("{group_id:int}")]
    public async Task<ActionResult> GetGroup([FromRoute(Name = "group_id")] int id)
    {
        try
        {
            var group = await _groupService.GetGroup(id);
            if (group == null)
            {
                return NotFound($"Group {id} doesn't exist");
            }
            return Ok(group);
        }
        catch (Exception e)
        {
            _logger.LogError("Getting group {} failed with unexpected error:{}", id, e.Message);
            return StatusCode(500, "Unexpected error");
        }

    }

    /// <summary>
    /// Create a group
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreateGroup([FromBody] GroupDto groupRequest)
    {

        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        if (username != groupRequest.Creator.Username)
        {
            return BadRequest("You can only create group for yourself");
        }

        try
        {
            var group = await _groupService.CreateGroup(username, groupRequest.Name);
            _logger.LogInformation("Group {} was created by {}", group.Name, username);
            return CreatedAtAction(nameof(CreateGroup), group);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Creating group {} failed with unexpected error:{}", groupRequest.Name, e.Message);
            return StatusCode(500, "Unexpected error");
        }
    }

    /// <summary>
    /// Add the user to the group
    /// </summary>
    [Authorize]
    [HttpPost("{group_id:int}/memberships")]
    public async Task<ActionResult> AddMember([FromRoute(Name = "group_id")] int groupId, [FromBody] MembershipDto membershipRequest)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        if (username != membershipRequest.Member.Username)
        {
            return BadRequest("You can only add yourself to a group");
        }
        try
        {
            var membership = await _groupService.AddMember(username, groupId);

            // Add connections
            var connections = _connectionService.GetConnections(username);

            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.AddToGroupAsync(connectionId, groupId.ToString())));

            // TODO: Remind group members 
            _logger.LogInformation("{} joined group {}", username, groupId);
            return CreatedAtAction(nameof(AddMember), membership);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("User {} failed to join group {} with unexpected error:{}", username, groupId, e.Message);
            return StatusCode(500, "Unexpected error");
        }
    }

    /// <summary>
    /// Remove the user from the group
    /// </summary>
    [Authorize]
    [HttpDelete("{group_id:int}/memberships")]
    public async Task<ActionResult> RemoveMember([FromRoute(Name = "group_id")] int groupId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _groupService.RemoveMember(username, groupId);

            // Remove connections
            var connections = _connectionService.GetConnections(username);
            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupId.ToString())));

            // TODO: Remind group members 
            _logger.LogInformation("User {} left the group {}", username, groupId);
            return Ok("Ok");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("User {} failed to leave the group {} with unexpected error:{}", username, groupId, e.Message);
            return StatusCode(500, "Unexpected error");
        }
    }
}