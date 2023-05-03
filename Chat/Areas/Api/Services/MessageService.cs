using Microsoft.EntityFrameworkCore;
using Chat.Areas.Api.Data;
using Chat.Areas.Api.Models;
using Chat.Common.DTOs; 
namespace Chat.Areas.Api.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _dbContext;


    public MessageService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveMessage(MessageDTO message)
    {
        var sender = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == message.Sender);

        if (message.Type == MessageType.GroupMessage)
        {
            var receiver = await _dbContext.Groups.FirstOrDefaultAsync(e => e.GroupName == message.Receiver);
            var dbMessage = new GroupMessage
                { Sender = sender!, Receiver = receiver!, Content = message.Content, SentTime = message.Time};
            _dbContext.GroupMessages.Add(dbMessage);
            await _dbContext.SaveChangesAsync();
            return dbMessage.Id;
        }
        else
        {
            var receiver = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == message.Receiver);
            var dbMessage = new UserMessage
                { Sender = sender!, Receiver = receiver!, Content = message.Content, SentTime = message.Time};
            _dbContext.UserMessages.Add(dbMessage);
            await _dbContext.SaveChangesAsync();
            return dbMessage.Id;
        }
    }
}