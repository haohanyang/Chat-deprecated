// https://github.com/nhooyr/websocket/blob/master/examples/chat/chat.go#L177

using Chat.Server.Hub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ChatServerController : Controller
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatServerController(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }
}