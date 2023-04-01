using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Configs;

public class UsernameBasedUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    }
}