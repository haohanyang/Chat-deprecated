using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Models;

public class UserMessage
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }

    public string SenderId { get; set; }
    public ApplicationUser Sender { get; set; }

    public string ReceiverId { get; set; }
    public ApplicationUser Receiver { get; set; }

    public string Content { get; set; }
    public DateTime Time { get; set; }
}

public class GroupMessage
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }

    public string SenderId { get; set; }
    public ApplicationUser Sender { get; set; }

    public string ReceiverId { get; set; }
    public Group Receiver { get; set; }

    public string Content { get; set; }
    public DateTime Time { get; set; }
}