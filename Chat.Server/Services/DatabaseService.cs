using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Services;

public interface IDatabaseService
{
    public Task ProcessTask(IDatabaseTask task);
}

public class DatabaseService : IDatabaseService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ILogger<DatabaseService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private int executionCount = 0;

    public DatabaseService(ILogger<DatabaseService> logger, UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _userManager = userManager;
    }

    public async Task ProcessTask(IDatabaseTask task)
    {
        if (task is AddUserMessageTask addUserMessageTask)
            await AddUserMessage(addUserMessageTask);
    }

    private async Task AddGroupMessage(AddGroupMessageTask task)
    {
        var cancellationToken = task.CancellationToken;
        var message = task.Message;

        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var dbSender = await _userManager.FindByNameAsync(message.Sender);
                if (dbSender == null)
                {
                    _logger.LogError("AddUserMessage {} error:u/{} not found in db", message.Content , message.Sender);
                    return;
                }

                var dbReceiver = await _applicationDbContext.Groups.FindAsync(message.Receiver);
                if (dbReceiver == null)
                {
                    
                    _logger.LogError("AddUserMessage {} error:g/{} not found in db", message.Content , message.Receiver);
                    return;
                }

                await _applicationDbContext.GroupMessages.AddAsync(new GroupMessage
                {
                    Content = message.Content,
                    SenderId = dbSender.Id,
                    ReceiverId = dbReceiver.Id,
                    Time = new DateTime()
                }, cancellationToken);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("AddUserMessage {}: ok", message.Content);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("AddUserMessage {} error:{}", message.Content, e.Message);
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
                var dbSender = await _userManager.FindByNameAsync(message.Sender);
                if (dbSender == null)
                {
                    _logger.LogError("AddGroupMessage {} error: u/{} not found in db", message.Content , message.Sender);
                    return;
                }

                var dbReceiver = await _userManager.FindByNameAsync(message.Receiver);
                if (dbReceiver == null)
                {
                    _logger.LogError("AddGroupMessage {} error:u/{} not found in db",  message.Content ,message.Receiver);
                    return;
                }

                await _applicationDbContext.UserMessages.AddAsync(new UserMessage
                {
                    Content = message.Content,
                    SenderId = dbSender.Id,
                    ReceiverId = dbReceiver.Id,
                    Time = new DateTime()
                }, cancellationToken);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("AddGroupMessage {} ok", message.Content);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("AddGroupMessage error:{}", e.Message);
        }
    }
}