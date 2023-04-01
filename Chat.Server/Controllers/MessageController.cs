// using Chat.Common;
// using Chat.Server.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.SignalR;
//
// namespace Chat.Server.Controllers;
//
// [ApiController]
// public class MessageController : Controller
// {
//     private readonly IDatabaseService _databaseService;
//     private readonly IHubContext<ChatHub, IChatClient> _hubContext;
//     private readonly ILogger<MessageController> _logger;
//
//     public MessageController(IHubContext<ChatHub, IChatClient> hubContext,
//         IDatabaseService databaseService,
//         ILogger<MessageController> logger)
//     {
//         _hubContext = hubContext;
//         _databaseService = databaseService;
//         _logger = logger;
//     }
//
//     [Authorize]
//     [HttpPost("send")]
//     public async Task<ActionResult> SendMessage([FromBody] Message message)
//     {
//         try
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest("Model is invalid");
//
//             if (message.Type == MessageType.UserMessage)
//             {
//                 var receiver = _databaseService.GetUser(message.Receiver);
//                 if (receiver?.UserName == null)
//                     return BadRequest("u/" + message.Receiver + " doesn't exist");
//                 await _hubContext.Clients.User(receiver.UserName).ReceiveMessage(message);
//                 _logger.LogInformation("u/{} -> u/{} : {}", message.Sender, message.Receiver, message.Content);
//             }
//             else
//             {
//                 var receiver = _databaseService.GetGroup(message.Receiver);
//                 if (receiver == null)
//                     return BadRequest("g/" + message.Receiver + " doesn't exist");
//                 await _hubContext.Clients.Group(message.Receiver).ReceiveMessage(message);
//                 _logger.LogInformation("u/{} -> g/{} : {}", message.Sender, message.Receiver, message.Content);
//             }
//
//             return Ok("ok");
//         }
//         catch (Exception e)
//         {
//             return BadRequest(e.Message);
//         }
//     }
// }