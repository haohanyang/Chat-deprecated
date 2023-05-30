using System.ComponentModel.DataAnnotations.Schema;
using Chat.Common.Dto;

namespace Chat.Domain;

public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string AuthorId { get; set; } = string.Empty;
    public User Author { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = new();

}
