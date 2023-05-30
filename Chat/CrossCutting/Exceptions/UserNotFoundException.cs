namespace Chat.CrossCutting.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string username) : base($"User {username} not found")
    {

    }
}