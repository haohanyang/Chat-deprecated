namespace Chat.Common.Dto;
public class GroupChannelDto : ChannelDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public IEnumerable<GroupChannelMembershipDto>? Memberships { get; init; }
    public UserDto Creator { get; init; } = null!;
}