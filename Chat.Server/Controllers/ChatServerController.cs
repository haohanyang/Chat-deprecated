// https://github.com/nhooyr/websocket/blob/master/examples/chat/chat.go#L177
// https://medium.com/geekculture/how-to-add-jwt-authentication-to-an-asp-net-core-api-84e469e9f019

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
    private readonly IAuthenticationService _authenticationService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ChatServerController> _logger;

    public ChatServerController(IHubContext<ChatHub,IChatClient> hubContext, 
        IAuthenticationService authService, 
        UserManager<IdentityUser> userManager, 
        ApplicationDbContext dbContext,
        ILogger<ChatServerController> logger)
    {
        _hubContext = hubContext;
        _authenticationService = authService;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthenticationRequest request) 
    {
        if (!ModelState.IsValid)
        {
            return BadRequest ("Model is invalid");
        }

        var result = await _userManager.CreateAsync(
            new IdentityUser { UserName = request.Username }, request.Password);
        
        if (result.Succeeded)
        {
            // Possibly duplicate
            await _dbContext.Members.AddAsync(new Member { MemberId = request.Username });
            await _dbContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(Register), new { username = request.Username }, request);
        }
        
        foreach (var error in result.Errors) {
            ModelState.AddModelError(error.Code, error.Description);
        }
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Authenticate([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var managedUser = await _userManager.FindByNameAsync(request.Username);
        if (managedUser is null)
        {
            return BadRequest("User " + request.Username + " doesn't exist");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
        {
            return BadRequest("Username or password is incorrect");
        }

        // var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == request.Username);
        // if (dbUser == null)
        // {
        //     return BadRequest("User " + request.Username + " doesn't exist");
        // }

        var accessToken = _authenticationService.CreateToken(managedUser);
        if (accessToken == null)
        {
            return BadRequest("Username doesn't exist");
        }
        
        // await _dbContext.SaveChangesAsync();
        return Ok(new AuthenticationResponse
        {
            Username = request.Username,
            Token = accessToken
        });
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<ActionResult> SendMessage([FromBody] Message message)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest ("Model is invalid");
            }
            
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null || !username.Equals(message.From))
            {
                return BadRequest("Invalid user");
            }
            
            // Check the receiver
            if (message.Type == ReceiverType.User)
            {
                var receiver = await _userManager.FindByNameAsync(message.To);
                if (receiver?.UserName == null)
                {
                    return BadRequest("u/" + message.To + " doesn't exist");
                }
                await _hubContext.Clients.User(receiver.UserName).ReceiveMessage(message);
                _logger.LogInformation("u/{} sent {} to u/{}", message.From, message.Content, message.To );
            }
            else
            { 
                var receiver = await _dbContext.Groups.FindAsync(message.To);
                if (receiver == null)
                {
                    return BadRequest("g/" + message.To + " doesn't exist");
                }

                await _hubContext.Clients.Group(message.To).ReceiveMessage(message);
                _logger.LogInformation("u/{} sent {} to g/{}", message.From, message.Content, message.To );
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
    public async Task<ActionResult> CreateGroup(/*[FromQuery(Name = "id")]*/ [FromBody] string groupId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest ("Model is invalid");
            }
            
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null)
            {
                return BadRequest("Invalid user");
            }
            
            var group = await _dbContext.Groups.FindAsync(groupId);
            if (group != null)
            {
                return BadRequest("g/" + groupId + " already exists");
            }

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
    public async Task<ActionResult> JoinGroup(/*[FromQuery(Name = "id")]*/ [FromBody] string groupId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest ("Model is invalid");
            }
            
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null)
            {
                return BadRequest("Invalid user");
            }
            
            var group = await _dbContext.Groups.FindAsync(groupId);
            if (group == null)
            {
                return BadRequest("g/" + groupId + " doesn't exists");
            }
            
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