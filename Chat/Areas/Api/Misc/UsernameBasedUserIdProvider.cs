using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Areas.Api.Misc;

public class UsernameBasedUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    }
}