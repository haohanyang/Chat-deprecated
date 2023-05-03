using System.Security.Claims;
using Chat.Common;
using Chat.Areas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Areas.Api.Controllers;

[Route("api/group")]
[ApiController]
public class UserGroupController : ControllerBase
{
    private readonly ILogger<UserGroupController> _logger;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly IUserGroupService _userGroupService;
    private readonly IConnectionService _connectionService;


    public UserGroupController(IUserGroupService userGroupService, IHubContext<ChatHub, IChatClient> hubContext, IConnectionService connectionService , ILogger<UserGroupController> logger)
    {
        _userGroupService = userGroupService;
        _hubContext = hubContext;
        _logger = logger;
        _connectionService = connectionService;
    }

    [Authorize]
    [HttpPost("create_group")]
    public async Task<ActionResult> CreateGroup([FromQuery(Name = "group_name")] string groupName)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        
        if (groupName.IsNullOrEmpty())
        {
            return BadRequest("Group name is null");
        }
        
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _userGroupService.CreateGroup(groupName);
            _logger.LogInformation("Group {} was created by {}", groupName, username);
            return CreatedAtAction(nameof(CreateGroup), new { GroupName = groupName });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Creating group {} failed with unknown error:{}", groupName ,e.Message);
            return BadRequest("Unknown error");
        }
    }


    [Authorize]
    [HttpPost("join_group")]
    public async Task<ActionResult> JoinGroup([FromQuery(Name = "group_name")] string groupName)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        if (groupName.IsNullOrEmpty())
        {
            return BadRequest("Group name is null");
        }
        
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _userGroupService.JoinGroup(username, groupName);
            
            // Add connections
            var connections = _connectionService.GetConnections(username);
            Console.WriteLine("connections of " + groupName);
            foreach (var connection in connections)
            {
                Console.WriteLine(connection);
            }
            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.AddToGroupAsync(connectionId, groupName)));
            
            // Remind group members 
            await _hubContext.Clients.Group(groupName).ReceiveNotification(new NotificationDTO
                { Content = "User {} joined the group", Time = new DateTime() });
            _logger.LogInformation("{} joined group {}", username, groupName);

            return CreatedAtAction(nameof(JoinGroup), new { GroupName = groupName, Username = username });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("User {} joining group {} failed with unknown error:{}",username,groupName, e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [Authorize]
    [HttpPost("leave_group")]
    public async Task<ActionResult> LeaveGroup([FromQuery(Name = "group_name")] string groupName)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        if (groupName.IsNullOrEmpty())
        {
            return BadRequest("Group name is null");
        }
        
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _userGroupService.LeaveGroup(username, groupName);
            
            // Remove connections
            var connections = _connectionService.GetConnections(username);
            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName)));
            
            // Remind group members 
            await _hubContext.Clients.Group(groupName).ReceiveNotification(new NotificationDTO
                { Content = "User {} left the group", Time = new DateTime() });
            _logger.LogInformation("{} left the group {}", username, groupName);
            return Ok("Ok");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("User {} leaving the group {} failed with unknown error:{}",username,groupName, e.Message);
            return BadRequest(e.Message);
        }
    }
}