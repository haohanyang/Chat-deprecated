using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Models;

public class UsernameBasedUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    }
}