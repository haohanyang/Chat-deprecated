using Microsoft.EntityFrameworkCore;
using Chat.Areas.Api.Data;
using Chat.Areas.Api.Models;
using Chat.Common.DTOs;
namespace Chat.Areas.Api.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MessageService> _logger;

    public MessageService(ApplicationDbContext dbContext, ILogger<MessageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> SaveMessage(MessageDTO message)
    {
        var sender = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == message.Sender.Username);

        if (sender == null)
        {
            throw new ArgumentException("User " + message.Sender + " doesn't exist");
        }

        if (message.Type == MessageType.GroupMessage)
        {
            var receiver = await _dbContext.Groups.FirstOrDefaultAsync(e => e.GroupName == message.Receiver.Username);
            if (receiver == null)
            {
                throw new ArgumentException("Group " + message.Receiver + " doesn't exist");
            }
            var dbMessage = new GroupMessage
            {
                Sender = sender,
                Receiver = receiver,
                Content = message.Content,
                SentTime = message.Time,
            };
            _dbContext.GroupMessages.Add(dbMessage);
            await _dbContext.SaveChangesAsync();
            return dbMessage.Id;
        }
        else
        {
            var receiver = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == message.Receiver.Username);
            if (receiver == null)
            {
                throw new ArgumentException("User " + message.Receiver + " doesn't exist");
            }
            var dbMessage = new UserMessage
            {
                Sender = sender,
                Receiver = receiver,
                Content = message.Content,
                SentTime = message.Time
            };
            _dbContext.UserMessages.Add(dbMessage);
            await _dbContext.SaveChangesAsync();
            return dbMessage.Id;
        }
    }

    public async Task<IEnumerable<MessageDTO>> GetUserChat(string username1, string username2)
    {
        var user1 = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username1);
        if (user1 == null)
        {
            throw new ArgumentException("User " + username1 + " doesn't exist");

        }
        var user2 = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username2);
        if (user2 == null)
        {
            throw new ArgumentException("User " + username2 + " doesn't exist");
        }
        var query = from m in _dbContext.UserMessages
                    where (m.Sender == user1 && m.Receiver == user2) || (m.Sender == user2 && m.Receiver == user1)
                    select m;

        var messages = await query.ToArrayAsync();
        return messages.Select(e => new MessageDTO
        {
            Sender = e.SenderId == user1.Id ? user1.ToDTO() : user2.ToDTO(),
            Receiver = e.ReceiverId == user1.Id ? user1.ToDTO() : user2.ToDTO(),
            Content = e.Content,
            Time = e.SentTime,
            Type = MessageType.UserMessage
        });
    }
}