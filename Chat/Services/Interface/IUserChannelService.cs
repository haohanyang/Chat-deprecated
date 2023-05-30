using Chat.Common.Dto;
namespace Chat.Services.Interface;

public interface IUserChannelService : IChannelService
{
    public Task<UserChannelDto> CreateChannel(string username, string channelName);
    public Task<IEnumerable<UserChannelDto>> GetAllChannels();
    public Task<UserChannelDto> GetChannel(int channelId);
    /// <summary>
    /// Get all user channels that the user is a member of
    /// </summary>
    public Task<IEnumerable<UserChannelDto>> GetChannels(string username);
    public Task<UserChannelDto> GetChannel(string username1, string username2);
}