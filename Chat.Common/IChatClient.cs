using Chat.Common.DTOs;
namespace Chat.Common;
public interface IChatClient
{
    Task ReceiveUserMessage(UserMessageDTO message);
    Task ReceiveGroupMessage(GroupMessageDTO message);
    Task ReceiveNotification(NotificationDTO notification);
}