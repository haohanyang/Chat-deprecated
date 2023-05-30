namespace Chat.CrossCutting.Exceptions;

public class InvalidUsernameException : ArgumentException
{
    public InvalidUsernameException(string username) : base($"Username {username} is invalid")
    {

    }
}