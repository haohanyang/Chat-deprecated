namespace Chat.Common.DTOs;

public class MessageDTO
{
    public int Id { get; set; }
    public UserDTO Sender { get; set; } = new();
    public DateTime Time { get; set; } = DateTime.Now;
    public string Content { get; set; } = string.Empty;

}

public class UserMessageDTO : MessageDTO
{
    public UserDTO Receiver { get; set; } = new();
}

public class GroupMessageDTO : MessageDTO
{
    public GroupDTO Receiver { get; set; } = new();
}