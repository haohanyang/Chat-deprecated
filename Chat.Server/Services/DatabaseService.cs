using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Services;

public interface IDatabaseService
{
    public Task ProcessTask(IDatabaseTask task);
    public (bool, ISet<string>) GetUserGroups(string username);
    public (bool, ISet<string>) GetGroupMembers(string group);
}

public class DatabaseService : IDatabaseService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ILogger<DatabaseService> _logger;


    public DatabaseService(ILogger<DatabaseService> logger,
        ApplicationDbContext applicationDbContext)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
    }

    public async Task ProcessTask(IDatabaseTask task)
    {
        if (task is AddUserMessageTask addUserMessageTask)
            await AddUserMessage(addUserMessageTask);
        if (task is AddGroupMessageTask addGroupMessageTask)
            await AddGroupMessage(addGroupMessageTask);
        if (task is AddGroupTask addGroupTask)
            await AddGroup(addGroupTask);
        if (task is AddMemberTask addMemberTask)
            await AddMember(addMemberTask);
        if (task is RemoveMemberTask removeMemberTask)
            await RemoveMember(removeMemberTask);
    }

    public (bool, ISet<string>) GetUserGroups(string username)
    {
        var user = _applicationDbContext.Users.Include(e => e.Groups).FirstOrDefault(e => e.UserName == username);
        if (user != null)
        {
            var groups = user.Groups.Select(e => e.Id).ToHashSet();
            return (true, groups);
        }

        return (false, new HashSet<string>());
    }

    public (bool, ISet<string>) GetGroupMembers(string groupId)
    {
        var group = _applicationDbContext.Groups.Include(e => e.Members).FirstOrDefault(e => e.Id == groupId);
        if (group != null) 
            return (true, group.Members.Select(e => e.UserName!).ToHashSet());
        return (false, new HashSet<string>());
    }


    private async Task AddGroupMessage(AddGroupMessageTask task)
    {
        var cancellationToken = task.CancellationToken;
        var message = task.Message;

        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var dbSender = await _applicationDbContext.Users.
                    FirstOrDefaultAsync(e => e.UserName == message.Sender, cancellationToken); 
                // _userManager.FindByNameAsync(message.Sender);
                if (dbSender == null)
                {
                    _logger.LogError("AddGroupMessage {} error:u/{} not found in db", message.Content, message.Sender);
                    return;
                }

                var dbReceiver = await _applicationDbContext.Groups.FindAsync(message.Receiver, cancellationToken);
                if (dbReceiver == null)
                {
                    _logger.LogError("AddGroupMessage error:{}", "group doesn't exist");
                    return;
                }
                
                dbReceiver.Messages.Add(new GroupMessage
                {
                    Content = message.Content,
                    SenderId = dbSender.Id,
                    ReceiverId = dbReceiver.Id,
                    Time = new DateTime()
                });
                await _applicationDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("AddGroupMessage {}: ok", message.Content);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("AddGroupMessage {} error:{}", message.Content, e.Message);
        }
    }

    private async Task AddUserMessage(AddUserMessageTask task)
    {
        var cancellationToken = task.CancellationToken;
        var message = task.Message;
        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var dbSender = await _applicationDbContext.Users.
                    FirstOrDefaultAsync(e => e.UserName == message.Sender, cancellationToken);
                //_userManager.FindByNameAsync(message.Sender);
                if (dbSender == null)
                {
                    _logger.LogError("AddUserMessage {} error: u/{} not found in db", message.Content, message.Sender);
                    return;
                }

                var dbReceiver =
                    await _applicationDbContext.Users.
                        FirstOrDefaultAsync(e => e.UserName == message.Receiver,
                        cancellationToken);
                if (dbReceiver == null)
                {
                    _logger.LogError("AddUserMessage {} error:u/{} not found in db", message.Content, message.Receiver);
                    return;
                }
                
                dbSender.UserMessagesSent.Add(new UserMessage
                {
                    Content = message.Content,
                    SenderId = dbSender.Id,
                    ReceiverId = dbReceiver.Id,
                    Time = new DateTime()
                });
                
                await _applicationDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("AddUserMessage {} ok", message.Content);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("AddUserMessage error:{}", e.Message);
        }
    }


    private async Task AddGroup(AddGroupTask task)
    {
        var cancellationToken = task.CancellationToken;
        var groupId = task.GroupId;
        try
        {
            await _applicationDbContext.Groups.AddAsync(new Group
            {
                Id = groupId
            }, cancellationToken);
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("AddGroup {} ok", groupId);
        }
        catch (Exception e)
        {
            _logger.LogError("AddGroup {} error:{}", groupId, e.Message);
        }
    }

    private async Task AddMember(AddMemberTask task)
    {
        var cancellationToken = task.CancellationToken;
        try
        {
            var dbMember = await _applicationDbContext.Users.
                FirstOrDefaultAsync(e => e.UserName == task.MemberId, cancellationToken);
            if (dbMember == null)
            {
                _logger.LogError("AddMember {} to {} error: user doesn't exist", task.MemberId, task.GroupId);
                return;
            }

            var group = await _applicationDbContext.Groups.FindAsync(task.GroupId);
            if (group != null)
            {
                dbMember.Groups.Add(group);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "AddMember ok");
            }
            else
            {
                _logger.LogError("AddMember error:{}", "g/" + task.GroupId + " doesn't exist");
            }
            
        }
        catch (Exception e)
        {
            _logger.LogError("AddMember error:{}", e.Message);
        }
    }

    private async Task RemoveMember(RemoveMemberTask task)
    {
        var cancellationToken = task.CancellationToken;
        try
        {
            var dbMember =
                await _applicationDbContext.Users.Include(e => e.Groups). 
                    FirstOrDefaultAsync(e => e.UserName == task.MemberId,
                    cancellationToken);
            if (dbMember == null)
            {
                _logger.LogError("RemoveMember {} from {} error: user doesn't exist", task.MemberId, task.GroupId);
                return;
            }

            var group = dbMember.Groups.Find(e => e.Id == task.GroupId);
            if (group == null)
            {
                _logger.LogError("RemoveMember {} from {} error: group doesn't exist", task.MemberId, task.GroupId);
                return;
            }
            dbMember.Groups.Remove(group);
            
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("RemoveMember ok");
        }
        catch (Exception e)
        {
            _logger.LogError("RemoveMember error:{}", e.Message);
        }
    }
}