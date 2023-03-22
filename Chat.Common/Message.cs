namespace Chat.Common;

public readonly record struct Message(string From, string To, DateTime Time, ReceiverType Type, string Content);

public enum ReceiverType
{
    User,
    Group
}