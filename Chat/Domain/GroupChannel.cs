namespace Chat.Domain;

public class GroupChannel : Channel
{
    public string Name { get; set; } = string.Empty;

    public User Creator { get; set; } = null!;
    public string CreatorId { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public IEnumerable<GroupChannelMembership> Memberships { get; } = new List<GroupChannelMembership>();
    public IEnumerable<GroupMessage> Messages { get; } = new List<GroupMessage>();
}