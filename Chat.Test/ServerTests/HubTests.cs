// using Chat.Server.Services;
//
//
// namespace Chat.Test.ServerTests;
//
// using System.Dynamic;
// using NUnit.Framework;
// using Moq;
// public class HubTests
// {
//     [Test]
//     public void HubsAreMockableViaDynamic()
//     {
//         bool sendCalled = false;
//         var hub = new ChatHub();
//         var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
//         hub.Clients = mockClients.Object;
//         dynamic all = new ExpandoObject();
//         all.broadcastMessage = new Action<string, string>((name, message) => {
//             sendCalled = true;
//         });
//         mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
//         hub.Send("TestUser", "TestMessage");
//         Assert.True(sendCalled);
//     }
//     
// }