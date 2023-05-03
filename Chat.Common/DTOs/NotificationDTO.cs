namespace Chat.Common;

public record struct NotificationDTO(DateTime Time, string Content);

public enum NotificationType
{
    ERROR,
    WARNING,
    SUCCESS
}