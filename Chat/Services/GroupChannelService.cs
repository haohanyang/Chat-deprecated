using Chat.Common.Dto;
using Chat.CrossCutting.Exceptions;
using Chat.Data;
using Chat.Domain;
using Chat.Services.Interface;
using Microsoft.EntityFrameworkCore;
namespace Chat.Services;

public class GroupChannelService : IGroupChannelService
{
    private readonly ApplicationDbContext _dbContext;

    public GroupChannelService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GroupChannelDto> CreateChannel(string username, string channelName)
    {
        var creator = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        if (creator == null)
        {
            throw new InvalidUsernameException(username);
        }

        var channel = new GroupChannel
        {
            Name = channelName,
            Creator = creator
        };

        _dbContext.GroupChannels.Add(channel);
        await _dbContext.SaveChangesAsync();

        return new GroupChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            Creator = creator.ToDto(),
            CreatedAt = channel.CreatedAt
        };
    }

    public Task<GroupChannelMembershipDto> AddMember(int groupId, string username)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroupChannelDto>> GetAllChannels()
    {
        throw new NotImplementedException();
    }

    public Task<GroupChannelDto> GetChannel(int groupId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroupChannelDto>> GetChannels(string memberUsername)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroupChannelMembershipDto>> GetMemberships(int groupId)
    {
        throw new NotImplementedException();
    }
}