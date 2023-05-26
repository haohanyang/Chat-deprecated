using Chat.Common.DTOs;
namespace Chat.Common;
public interface IChatClient
{
    Task ReceiveUserMessage(UserMessageDto message);
    Task ReceiveGroupMessage(GroupMessageDto message);
}