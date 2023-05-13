using System.ComponentModel.DataAnnotations.Schema;
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Models;

public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string SenderId { get; set; } = string.Empty;
    public User Sender { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime SentTime { get; set; } = new();
}

public class UserMessage : Message
{
    public string ReceiverId { get; set; } = string.Empty;
    public User Receiver { get; set; } = null!;

    public UserMessageDTO ToDto()
    {
        return new()
        {
            Sender = Sender.ToDto(),
            Receiver = Receiver.ToDto(),
            Content = Content,
            Time = SentTime
        };
    }
}

public class GroupMessage : Message
{
    public int ReceiverId { get; set; }
    public Group Receiver { get; set; } = null!;
    public GroupMessageDTO ToDto()
    {
        return new()
        {
            Sender = Sender.ToDto(),
            Receiver = Receiver.ToDto(),
            Content = Content,
            Time = SentTime
        };
    }
}