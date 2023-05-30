namespace Chat.Common.Dto;

public class MessageDto
{
    public int Id { get; init; }
    public UserDto Author { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public string Content { get; init; } = string.Empty;
    public int ChannelId { get; init; }
    public bool Read { get; set; } = true;
    public bool Delivered { get; set; } = true;
    public bool Sent { get; set; } = true;

}