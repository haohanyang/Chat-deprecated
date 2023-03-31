using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Controllers;

[ApiController]
public class MessageController : Controller
{
    private readonly IDatabaseService _databaseService;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ILogger<MessageController> _logger;

    public MessageController(IHubContext<ChatHub, IChatClient> hubContext,
        IDatabaseService databaseService,
        ILogger<MessageController> logger)
    {
        _hubContext = hubContext;
        _databaseService = databaseService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<ActionResult> SendMessage([FromBody] Message message)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is invalid");

            if (message.Type == ReceiverType.User)
            {
                var receiver = _databaseService.GetUser(message.To);
                if (receiver?.UserName == null)
                    return BadRequest("u/" + message.To + " doesn't exist");
                await _hubContext.Clients.User(receiver.UserName).ReceiveMessage(message);
                _logger.LogInformation("u/{} -> u/{} : {}", message.From, message.To, message.Content);
            }
            else
            {
                var receiver = _databaseService.GetGroup(message.To);
                if (receiver == null)
                    return BadRequest("g/" + message.To + " doesn't exist");
                await _hubContext.Clients.Group(message.To).ReceiveMessage(message);
                _logger.LogInformation("u/{} -> g/{} : {}", message.From, message.To, message.Content);
            }

            return Ok("ok");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}