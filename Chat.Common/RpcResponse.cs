namespace Chat.Common;

public record RpcResponse(RpcResponseStatus Status, string Message);

public enum RpcResponseStatus
{
    Warning,
    Success,
    Error
}