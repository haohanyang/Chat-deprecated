namespace Chat.Common.Dto;

public class UserChannelDto : ChannelDto
{
    public UserDto? User1 { get; init; } = new();
    public UserDto? User2 { get; init; } = new();

}