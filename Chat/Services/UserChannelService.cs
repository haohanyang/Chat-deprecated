using Chat.Common.Dto;
using Chat.Services.Interface;
using Chat.CrossCutting.Exceptions;
using Chat.Data;
using Chat.Domain;
using Microsoft.EntityFrameworkCore;
namespace Chat.Services;

public class UserChannelService : IUserChannelService
{
    private readonly ApplicationDbContext _dbContext;
    public UserChannelService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserChannelDto> CreateChannel(string username1, string username2)
    {
        var user1 = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username1);
        if (user1 == null)
        {
            throw new InvalidUsernameException(username1);
        }
        var user2 = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username2);
        if (user2 == null)
        {
            throw new InvalidUsernameException(username2);
        }

        UserChannel? channel;
        if (string.Compare(username1, username2) < 0)
        {
            channel = await _dbContext.UserChannels.FirstOrDefaultAsync(e => e.User1.UserName == username1 && e.User2.UserName == username2);
        }
        else
        {
            channel = await _dbContext.UserChannels.FirstOrDefaultAsync(e => e.User2.UserName == username1 && e.User1.UserName == username2);
        }

        if (channel != null)
        {
            throw new ChannelAlreadyExistsException(username1, username2);
        }

        channel = new UserChannel(user1, user2);
        _dbContext.UserChannels.Add(channel);
        await _dbContext.SaveChangesAsync();
        return new UserChannelDto
        {
            Id = channel.Id,
            User1 = user1.ToDto(),
            User2 = user2.ToDto(),
        };
    }

    public async Task<IEnumerable<UserChannelDto>> GetAllChannels()
    {
        var channels = await _dbContext.UserChannels.ToArrayAsync();
        return channels.Select(c => c.ToDto());
    }

    public async Task<UserChannelDto> GetChannel(int groupId)
    {
        var channel = await _dbContext.UserChannels.Include(c => c.User1).Include(e => e.User2).FirstOrDefaultAsync(e => e.Id == groupId);
        if (channel == null)
        {
            throw new ChannelNotFoundException(groupId);
        }
        return channel.ToDto();
    }

    public async Task<UserChannelDto> GetChannel(string username1, string username2)
    {
        var user1 = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username1);
        if (user1 == null)
        {
            throw new UserNotFoundException(username1);
        }
        var user2 = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username2);
        if (user2 == null)
        {
            throw new UserNotFoundException(username2);
        }

        UserChannel? channel;
        if (string.Compare(username1, username2) < 0)
        {
            channel = await _dbContext.UserChannels.FirstOrDefaultAsync(e => e.User1.UserName == username1 && e.User2.UserName == username2);
        }
        else
        {
            channel = await _dbContext.UserChannels.FirstOrDefaultAsync(e => e.User2.UserName == username1 && e.User1.UserName == username2);
        }

        if (channel == null)
        {
            throw new ChannelNotFoundException(username1, username2);
        }
        return channel.ToDto();
    }

    public Task<IEnumerable<UserChannelDto>> GetChannels(string username)
    {
        throw new NotImplementedException();
    }
}