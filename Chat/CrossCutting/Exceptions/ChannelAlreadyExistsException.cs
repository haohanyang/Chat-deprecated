namespace Chat.CrossCutting.Exceptions;

public class ChannelAlreadyExistsException : ArgumentException
{
    public ChannelAlreadyExistsException(int channelId) : base($"Channel {channelId} already exists")
    {

    }

    public ChannelAlreadyExistsException(string username1, string username2) : base($"Channel between {username1} and {username2} already exists")
    {

    }
}