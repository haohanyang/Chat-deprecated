namespace Chat.Common.DTOs;

public readonly record struct MessageDTO(string Sender, string Receiver, DateTime Time, MessageType Type, string Content);

public enum MessageType
{
    UserMessage,
    GroupMessage
}