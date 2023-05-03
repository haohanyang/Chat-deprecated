using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Areas.Api.Models;

public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string SenderId { get; set; }
    public User Sender { get; set; }

    public string Content { get; set; }
    public DateTime SentTime { get; set; } = new();
}

public class UserMessage : Message
{
    public string ReceiverId { get; set; }
    public User Receiver { get; set; }
}

public class GroupMessage : Message
{
    public int ReceiverId { get; set; }
    public Group Receiver { get; set; }
}