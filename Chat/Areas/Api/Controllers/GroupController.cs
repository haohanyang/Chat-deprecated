using System.Security.Claims;
using Chat.Common;
using Chat.Areas.Api.Services;
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

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDTO>>> GetAllGroups()
    {
        var groups = await _groupService.GetAllGroups();
        return Ok(groups);
    }


    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var groupName = request.GroupName;
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            var id = await _groupService.CreateGroup(username, groupName);
            _logger.LogInformation("Group {} was created by {}", groupName, username);
            return CreatedAtAction(nameof(CreateGroup), new { GroupName = groupName, Id = id });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Creating group {} failed with unknown error:{}", groupName, e.Message);
            return BadRequest("Unknown error");
        }
    }


    [Authorize]
    [HttpPost("{group_id:int}/members")]
    public async Task<ActionResult> JoinGroup([FromRoute(Name = "group_name")] int groupId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            var membershipId = await _groupService.JoinGroup(username, groupId);

            // Add connections
            var connections = _connectionService.GetConnections(username);

            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.AddToGroupAsync(connectionId, groupId.ToString())));

            // Remind group members 
            await _hubContext.Clients.Group(groupId.ToString()).ReceiveNotification(
                new NotificationDTO
                {
                    Content = "User @{} joined the group",
                    Time = new DateTime()
                });

            _logger.LogInformation("{} joined group {}", username, groupId);
            return CreatedAtAction(nameof(JoinGroup), new { GroupId = groupId, Username = username, Id = membershipId });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("User {} failed to joining group {} with unknown error:{}", username, groupId, e.Message);
            return BadRequest(e.Message);
        }
    }


    [Authorize]
    [HttpDelete("{group_id:int}/members")]
    public async Task<ActionResult> LeaveGroup([FromRoute(Name = "group_id")] int groupId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _groupService.LeaveGroup(username, groupId);

            // Remove connections
            var connections = _connectionService.GetConnections(username);
            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupId.ToString())));

            // Remind group members 
            await _hubContext.Clients.Group(groupId.ToString()).ReceiveNotification(
                new NotificationDTO
                { Content = "User @{} left the group", Time = new DateTime() });
            _logger.LogInformation("User {} left the group {}", username, groupId);
            return Ok("Ok");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("User {} failed to leave the group {} with unknown error:{}", username, groupId, e.Message);
            return BadRequest(e.Message);
        }
    }
}