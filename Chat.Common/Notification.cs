namespace Chat.Common;

public record struct Notification(DateTime Time, string Content);

public enum NotificationType
{
    ERROR,
    WARNING,
    SUCCESS
}