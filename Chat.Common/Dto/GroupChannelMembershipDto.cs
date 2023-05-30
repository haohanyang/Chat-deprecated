namespace Chat.Common.Dto;

public class GroupChannelMembershipDto
{
    public int Id { get; set; }
    public UserDto Member { get; set; } = new();
    public int ChannelId { get; set; }
}