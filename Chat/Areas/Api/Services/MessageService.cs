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
            throw new ArgumentException("User " + message.Sender.Username + " doesn't exist");
        }

        if (message is UserMessageDTO userMessage)
        {
            var receiver = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == userMessage.Receiver.Username);
            if (receiver == null)
            {
                throw new ArgumentException("User " + userMessage.Receiver.Username + " doesn't exist");
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
        else
        {
            var groupMessage = message as GroupMessageDTO;
            var group = await _dbContext.Groups.FindAsync(groupMessage!.Receiver.Id);
            if (group == null)
            {
                throw new ArgumentException($"Group {groupMessage!.Receiver.Name} doesn't exist");
            }
            var dbMessage = new GroupMessage
            {
                Sender = sender,
                Receiver = group,
                Content = message.Content,
                SentTime = message.Time,
            };
            _dbContext.GroupMessages.Add(dbMessage);
            await _dbContext.SaveChangesAsync();
            return dbMessage.Id;
        }
    }

    public async Task<IEnumerable<GroupMessageDTO>> GetGroupChat(int id)
    {
        var group_ = await _dbContext.Groups.FindAsync(id);
        if (group_ == null)
        {
            throw new ArgumentException($"Group {id} doesn't exist");
        }
        var query = from m in _dbContext.GroupMessages
                    where m.Receiver == group_
                    select m;

        var messages = await query.Include(m => m.Sender).ToArrayAsync();
        return messages.Select(e => new GroupMessageDTO
        {
            Id = e.Id,
            Sender = e.Sender.ToDto(),
            Receiver = group_.ToDto(),
            Content = e.Content,
            Time = e.SentTime,
        });
    }

    public async Task<IEnumerable<UserMessageDTO>> GetUserChat(string username1, string username2)
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
        return messages.Select(e => new UserMessageDTO
        {
            Id = e.Id,
            Sender = e.SenderId == user1.Id ? user1.ToDto() : user2.ToDto(),
            Receiver = e.ReceiverId == user1.Id ? user1.ToDto() : user2.ToDto(),
            Content = e.Content,
            Time = e.SentTime,
        });
    }
}