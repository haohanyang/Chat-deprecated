using Chat.Common.Dto;

namespace Chat.Services.Interface;

public interface IGroupChannelService
{
    public Task<GroupChannelDto> CreateChannel(string username, string channelName);
    public Task<IEnumerable<GroupChannelDto>> GetAllChannels();
    public Task<GroupChannelDto> GetChannel(int groupId);

    /// <summary>
    /// Get all channels that the user is a member of
    /// </summary>
    public Task<IEnumerable<GroupChannelDto>> GetChannels(string memberUsername);
    public Task<IEnumerable<GroupChannelMembershipDto>> GetMemberships(int groupId);
    public Task<GroupChannelMembershipDto> AddMember(int groupId, string username);
}