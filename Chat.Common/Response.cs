namespace Chat.Common;

public readonly record struct Response(ResponseType Type, string ServerMessage, string ClientMessage);

public enum ResponseType
{
    Success,
    Warning,
    Error
}