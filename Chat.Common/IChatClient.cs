using Chat.Common.Dto;
namespace Chat.Common;
public interface IChatClient
{
    Task ReceiveUserMessage(MessageDto message);
    Task ReceiveGroupMessage(MessageDto message);
}