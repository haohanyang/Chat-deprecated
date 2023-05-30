namespace Chat.CrossCutting.Exceptions;

public class InvalidChannelException : ArgumentException
{
    public InvalidChannelException(int channelId) : base($"Channel {channelId} is invalid")
    {

    }
}