using Chat.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Chat.Test.ServerTests.Services;

public class UserGroupServiceTest : IClassFixture<TestDatabaseFixture>
{
    public UserGroupServiceTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    private TestDatabaseFixture Fixture { get; }

    [Fact]
    public async void TestCreateNonExistingGroup()
    {
        var groupName = "group-x";
        await using var dbContext = Fixture.CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<UserGroupService>();
        var userGroupService = new UserGroupService(dbContext, logger);

        await dbContext.Database.BeginTransactionAsync();
        await userGroupService.CreateGroup(groupName);
        dbContext.ChangeTracker.Clear();
        
        var groupNames = dbContext.Groups.Select(e => e.GroupName).ToList();
        Assert.Contains(groupName, groupNames);
    }
    
    [Fact]
    public async void TestCreateExistingGroup()
    {
        const string groupName = "group1";
        await using var dbContext = Fixture.CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<UserGroupService>();
        var userGroupService = new UserGroupService(dbContext, logger);

        await dbContext.Database.BeginTransactionAsync();
        await userGroupService.CreateGroup(groupName);
        await Assert.ThrowsAsync<ArgumentException>(async () => await userGroupService.CreateGroup(groupName));
        dbContext.ChangeTracker.Clear();
    }
    
    [Fact]
    public async void TestJoinGroup()
    {
        const string username = "user2";
        const string groupName = "group3";
        
        await using var dbContext = Fixture.CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<UserGroupService>();
        var userGroupService = new UserGroupService(dbContext, logger);

        await dbContext.Database.BeginTransactionAsync();
        await userGroupService.JoinGroup(username, groupName);
        dbContext.ChangeTracker.Clear();

        var membership = await dbContext.Memberships.FirstOrDefaultAsync(e => e.User.UserName == username && e.Group.GroupName == groupName);
        Assert.NotNull(membership);
    }
    
    [Fact]
    public async void TestLeaveGroup()
    {
        const string username = "user1";
        const string groupName = "group1";
        
        await using var dbContext = Fixture.CreateDbContext();
        var logger = new LoggerFactory().CreateLogger<UserGroupService>();
        var userGroupService = new UserGroupService(dbContext, logger);

        await dbContext.Database.BeginTransactionAsync();
        await userGroupService.LeaveGroup(username, groupName);
        dbContext.ChangeTracker.Clear();

        var membership = await dbContext.Memberships.FirstOrDefaultAsync(e => e.User.UserName == username && e.Group.GroupName == groupName);
        Assert.Null(membership);
    }
    
}