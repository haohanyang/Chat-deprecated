namespace Chat.Common.Logging;

public class ResponseBuilder
{
    public static Response GroupCreatedMessage(string userId)
    {
        var message = "Group " + userId + " is created";
        return new Response(ResponseType.Success, message,message);
    }
    
    public static Response GroupAlreadyExistsMessage(string userId)
    {
        var message = "Group " + userId + " already exists";
        return new Response(ResponseType.Warning, message,message);
    }
    
    public static Response UserNotExistsMessage(string userId)
    {
        var message = "User " + userId + " doesn't exist";
        return new Response(ResponseType.Error, message,message);
    }
    public static Response GroupNotExistsMessage(string groupId)
    {
        var message = "Group " + groupId + " doesn't exist";
        return new Response(ResponseType.Error, message,message);
    }
    
    public static Response JoinGroupMessage(string userId, string groupId
    )
    {
        return new Response(ResponseType.Success, "User " + userId + " has joined group " + groupId,
            "You have joined group " + groupId);
    }
    

    public static Response LeaveGroupMessage(string userId, string groupId
    )
    {
        return new Response(ResponseType.Success, "User " + userId + " has left group " + groupId,
            "You have left group " + groupId);
    }


    public static Response UserAlreadyInGroupMessage(string userId, string groupId
    )
    {
        return new Response(ResponseType.Warning, "User " + userId + " is already in group " + groupId,
            "You are already in group " + groupId);
    }

    public static Response UserNotInGroupMessage(string userId, string groupId, ResponseType type = ResponseType.Warning)
    {
        return new Response(type, "User " + userId + " is not in group " + groupId,
            "You are not in group " + groupId);
    }

}