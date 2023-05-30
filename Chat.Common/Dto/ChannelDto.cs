namespace Chat.Common.Dto;

public class ChannelDto
{
    public int Id { get; init; }
    public MessageDto? LastMessage { get; init; }
}