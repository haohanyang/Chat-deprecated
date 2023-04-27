using Chat.Common;
using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.EntityFrameworkCore;
using Message = Chat.Common.Message;

namespace Chat.Server.Services;

public interface IMessageService
{
    public Task SaveMessage(Message message);
}

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _dbContext;


    public MessageService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveMessage(Message message)
    {
        var sender = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == message.Sender);
        if (message.Type == MessageType.GroupMessage)
        {
            var receiver = await _dbContext.Groups.FirstOrDefaultAsync(e => e.GroupName == message.Receiver);
            _dbContext.GroupMessages.Add(new GroupMessage
                { Sender = sender!, Receiver = receiver!, Content = message.Content });
        }
        else
        {
            var receiver = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == message.Receiver);
            _dbContext.UserMessages.Add(new UserMessage
                { Sender = sender, Receiver = receiver, Content = message.Content });
        }
    }
}