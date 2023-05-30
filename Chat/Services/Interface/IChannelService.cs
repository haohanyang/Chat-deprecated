using Chat.Common.Dto;
namespace Chat.Services.Interface;

public interface IChannelService
{
    /// <summary>
    /// Get all memberships of the channel
    /// </summary>
    public Task<IEnumerable<GroupChannelMembershipDto>> GetMemberships(int channelId);

}