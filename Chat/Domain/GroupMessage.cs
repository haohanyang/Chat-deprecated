
namespace Chat.Domain;

public class GroupMessage : Message
{
    public int ChannelId { get; set; }
    public GroupChannel Channel { get; set; } = null!;
}