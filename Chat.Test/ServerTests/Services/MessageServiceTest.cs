using Chat.Common;
//using Chat.Server.Models;
using Chat.Server.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chat.Test.ServerTests.Services;

public class MessageServiceTest: IClassFixture<TestDatabaseFixture>
{
    
    public MessageServiceTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    private TestDatabaseFixture Fixture { get; }

    [Fact]
    public async void TestSendUserMessage()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var userGroupService = new MessageService(dbContext);

        const string username1 = "user1";
        const string username2 = "user2";
        
        await dbContext.Database.BeginTransactionAsync();
        await userGroupService.SaveMessage(new Message
        {
            Type = MessageType.UserMessage,
            Content = "test message",
            Sender = username1,
            Receiver = username2
        });
        
        dbContext.ChangeTracker.Clear();

        var user1 = await dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username1);
        var user2 = await dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username2);

        var message =
            await dbContext.UserMessages.FirstOrDefaultAsync(e => e.SenderId == user1.Id && e.ReceiverId == user2!.Id);
        Assert.NotNull(message);
    }
    
    [Fact]
    public async void TestSendGroupMessage()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var userGroupService = new MessageService(dbContext);
        
        const string username = "user1";
        const string groupName = "group1";
        
        await dbContext.Database.BeginTransactionAsync();
        await userGroupService.SaveMessage(new Message
        {
            Type = MessageType.GroupMessage,
            Content = "test message",
            Sender = username,
            Receiver = groupName
        });
        dbContext.ChangeTracker.Clear();
        
        var user = await dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        var group = await dbContext.Groups.FirstOrDefaultAsync(e => e.GroupName == groupName);

        var message =
            await dbContext.GroupMessages.FirstOrDefaultAsync(e => e.SenderId == user.Id && e.ReceiverId == group!.Id);
        
        Assert.NotNull(message);
    }
    
}