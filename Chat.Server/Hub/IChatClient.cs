using Chat.Common;

namespace Chat.Server.Hub;

public interface IChatClient
{
    Task ReceiveUserMessage(Message message);
    Task ReceiveGroupMessage(Message message);
    Task ReceiveNotification(Notification notification);
    Task RpcSucceeds(string message);
    Task RpcFails(string message);
    Task RpcWarns(string message);
    Task<string> GetMessage();
}