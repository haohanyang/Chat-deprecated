using System.Security.Claims;
using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Controllers;

public class UserGroupController : Controller
{
    private readonly ILogger<UserGroupController> _logger;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly UserGroupService _userGroupService;
    private readonly IConnectionService _connectionService;


    public UserGroupController(UserGroupService userGroupService, IHubContext<ChatHub, IChatClient> hubContext, IConnectionService connectionService , ILogger<UserGroupController> logger)
    {
        _userGroupService = userGroupService;
        _hubContext = hubContext;
        _logger = logger;
        _connectionService = connectionService;
    }

    [Authorize]
    [HttpPost("/api/create_group")]
    public async Task<ActionResult> CreateGroup([FromBody] string groupName)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _userGroupService.CreateGroup(groupName);
            _logger.LogInformation("Group {} was created by {}", groupName, username);
            return Ok("Ok");
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
    [HttpPost("/api/join_group")]
    public async Task<ActionResult> JoinGroup([FromBody] string groupName)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _userGroupService.JoinGroup(username, groupName);
            
            // Add connections
            var connections = _connectionService.GetConnections(username);
            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.AddToGroupAsync(connectionId, groupName)));
            
            // Remind group members 
            await _hubContext.Clients.Group(groupName).ReceiveNotification(new Notification
                { Content = "User {} joined the group", Time = new DateTime() });
            _logger.LogInformation("{} joined group {}", username, groupName);
            return Ok("Ok");
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
    [HttpPost("/api/leave_group")]
    public async Task<ActionResult> LeaveGroup([FromBody] string groupName)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model is invalid");
        var username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            await _userGroupService.LeaveGroup(username, groupName);
            
            // Remove connections
            var connections = _connectionService.GetConnections(username);
            await Task.WhenAll(connections.Select(connectionId =>
                _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName)));
            
            // Remind group members 
            await _hubContext.Clients.Group(groupName).ReceiveNotification(new Notification
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