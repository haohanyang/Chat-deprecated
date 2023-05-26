namespace Chat.Common.DTOs;

public class MessageDto
{
    public int Id { get; init; }
    public UserDto Sender { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public string Content { get; init; } = string.Empty;
    public bool Read { get; set; } = true;
    public bool Delivered { get; set; } = true;
}

public class UserMessageDto : MessageDto
{
    public UserDto Receiver { get; init; } = new();
}

public class GroupMessageDto : MessageDto
{
    public GroupDto Receiver { get; init; } = new();

}