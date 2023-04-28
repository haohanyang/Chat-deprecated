using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chat.Test.ServerTests.Services;

public class ChatHubTest : IClassFixture<TestDatabaseFixture>
{
    public ChatHubTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    private TestDatabaseFixture Fixture { get; }

    [Fact]
    public async void TestUserMessage()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var userGroupService = new UserGroupService(dbContext, new LoggerFactory().CreateLogger<UserGroupService>());
        var connectionService = new ConnectionService();

        var hub = new ChatHub(new LoggerFactory().CreateLogger<ChatHub>(), userGroupService, connectionService);
        
        var client1 = new Mock<IChatClient>();
        var client2 = new Mock<IChatClient>();

        client2.Setup(m => m.ReceiveMessage(It.IsAny<Message>())).Verifiable();
        
        var mockClients = new Mock<IHubCallerClients<IChatClient>>();
        hub.Clients = mockClients.Object;

        mockClients.Setup(m => m.User("user1")).Returns(client1.Object);
        mockClients.Setup(m => m.User("user2")).Returns(client2.Object);
        
        await hub.SendUserMessage("user1", "user2", "test-message");
    }
}