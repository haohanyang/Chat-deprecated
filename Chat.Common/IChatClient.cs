using Chat.Common.DTOs;
namespace Chat.Common;
public interface IChatClient
{
    Task ReceiveMessage(MessageDTO message);
    Task ReceiveNotification(NotificationDTO notification);
}