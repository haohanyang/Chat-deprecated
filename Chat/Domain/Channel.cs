using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Domain;

public class Channel {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = new();
}
