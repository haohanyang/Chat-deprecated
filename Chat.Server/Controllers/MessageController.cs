using System.Security.Claims;
using Chat.Common;
using Chat.Server.Data;
using Chat.Server.Models;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Controllers;

[ApiController]
public class ChatServerController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<ChatServerController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatServerController(IHubContext<ChatHub, IChatClient> hubContext,
        UserManager<IdentityUser> userManager,
        ApplicationDbContext dbContext,
        ILogger<ChatServerController> logger)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }


    [Authorize]
    [HttpPost("send")]
    public async Task<ActionResult> SendMessage([FromBody] Message message)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Model is invalid");

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null || !username.Equals(message.From)) return BadRequest("Invalid user");

            // Check the receiver
            if (message.Type == ReceiverType.User)
            {
                var receiver = await _userManager.FindByNameAsync(message.To);
                if (receiver?.UserName == null) return BadRequest("u/" + message.To + " doesn't exist");
                await _hubContext.Clients.User(receiver.UserName).ReceiveMessage(message);
                _logger.LogInformation("u/{} sent {} to u/{}", message.From, message.Content, message.To);
            }
            else
            {
                var receiver = await _dbContext.Groups.FindAsync(message.To);
                if (receiver == null) return BadRequest("g/" + message.To + " doesn't exist");

                await _hubContext.Clients.Group(message.To).ReceiveMessage(message);
                _logger.LogInformation("u/{} sent {} to g/{}", message.From, message.Content, message.To);
            }

            return Ok("Message is sent");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpPost("create_group")]
    public async Task<ActionResult> CreateGroup( /*[FromQuery(Name = "id")]*/ [FromBody] string groupId)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Model is invalid");

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null) return BadRequest("Invalid user");

            var group = await _dbContext.Groups.FindAsync(groupId);
            if (group != null) return BadRequest("g/" + groupId + " already exists");

            await _dbContext.Groups.AddAsync(new Group { GroupId = groupId });
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("u/{} creates group /g{}", username, groupId);

            return Ok("g/" + groupId + " is created");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }


    [Authorize]
    [HttpPost("join_group")]
    public async Task<ActionResult> JoinGroup( /*[FromQuery(Name = "id")]*/ [FromBody] string groupId)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Model is invalid");

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null) return BadRequest("Invalid user");

            var group = await _dbContext.Groups.FindAsync(groupId);
            if (group == null) return BadRequest("g/" + groupId + " doesn't exists");

            await _dbContext.Memberships.AddAsync(new Membership
            {
                MemberId = username,
                GroupId = groupId
            });

            await _dbContext.SaveChangesAsync();
            return Ok("Group " + groupId + " is created");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}