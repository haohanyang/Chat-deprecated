namespace Chat.CrossCutting.Exceptions;

public class ChannelNotFoundException : NotFoundException
{
    public ChannelNotFoundException(int channelId) : base($"Channel {channelId} not found")
    {

    }

    public ChannelNotFoundException(string username1, string username2) : base($"User channel between {username1} {username2} not found")
    {

    }
}