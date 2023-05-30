using Chat.Common.Dto;
namespace Chat.Domain;

public class UserMessage : Message
{
    public int ChannelId { get; set; }
    public UserChannel Channel { get; set; } = null!;

    public MessageDto ToDto()
    {
        return new MessageDto
        {
            Id = Id,
            Author = Author.ToDto(),
            CreatedAt = CreatedAt,
            Content = Content,
            ChannelId = ChannelId,
            Read = true,
            Delivered = true,
            Sent = true
        };
    }
}