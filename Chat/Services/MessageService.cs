using Microsoft.EntityFrameworkCore;
using Chat.Common.Dto;
using Chat.Data;
using Chat.Services.Interface;

namespace Chat.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _dbContext;


    public MessageService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IEnumerable<MessageDto>> GetGroupMessages(int channelId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<MessageDto>> GetUserMessages(int channelId)
    {
        throw new NotImplementedException();
    }

    public Task<MessageDto> SaveGroupMessage(MessageDto message)
    {
        throw new NotImplementedException();
    }

    public Task<MessageDto> SaveUserMessage(MessageDto message)
    {
        throw new NotImplementedException();
    }
}