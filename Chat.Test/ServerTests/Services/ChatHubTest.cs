using Chat.Common;
using Chat.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace Chat.Test.ServerTests.Services;

public class ChatHubTest
{
    
    [Fact]
    public async void TestUserMessage()
    {

        var userGroupService = new InMemoryUserGroupService();
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
        
        client2.Verify(m => m.ReceiveMessage(It.IsAny<Message>()), Times.Once);
    }

    [Fact]
    public async void TestGroupMessage()
    {
        var userGroupService = new InMemoryUserGroupService();
        await userGroupService.CreateGroup("group1");
        userGroupService.AddGroupMembers("group1", new List<string> { "user1", "user2"});
        var connectionService = new ConnectionService();

        var hub = new ChatHub(new LoggerFactory().CreateLogger<ChatHub>(), userGroupService, connectionService);

        var client1 = new Mock<IChatClient>();
        var client2 = new Mock<IChatClient>();

        var group = new Mock<IChatClient>();

        client1.Setup(m => m.ReceiveMessage(It.IsAny<Message>())).Verifiable();
        client2.Setup(m => m.ReceiveMessage(It.IsAny<Message>())).Verifiable();
        group.Setup(m => m.ReceiveMessage(It.IsAny<Message>())).Verifiable();

        var mockClients = new Mock<IHubCallerClients<IChatClient>>();
        hub.Clients = mockClients.Object;

        mockClients.Setup(m => m.User("user1")).Returns(client1.Object);
        mockClients.Setup(m => m.User("user2")).Returns(client2.Object);
        mockClients.Setup(m => m.Group("group1")).Returns(group.Object);
        await hub.SendGroupMessage("user1", "group1", "test-message");
        
        group.Verify(m => m.ReceiveMessage(It.IsAny<Message>()), Times.Once);
    }
}