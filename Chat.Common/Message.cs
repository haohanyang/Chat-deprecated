namespace Chat.Common;

public readonly record struct Message(string Sender, string Receiver, DateTime Time, MessageType Type, string Content);

public enum MessageType
{
    UserMessage,
    GroupMessage
}