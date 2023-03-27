using Chat.Common;

namespace Chat.Client.Command;

public interface ICommand
{
}

public class RegisterCommand : ICommand
{
    public string Username { get; init; }
    public string? Password { get; init; }
}

public class LoginCommand : ICommand
{
    public string Username { get; init; }
    public string? Password { get; init; }
}
public class SendMessageCommand : ICommand
{
    public string  Receiver { get; init; }
    public ReceiverType ReceiverType { get; init; }
    public string Message { get; init; }
}

public class JoinGroupCommand : ICommand
{
    public string GroupId { get; init; }
}

public class LeaveGroupCommand : ICommand
{
    public string GroupId { get; init; }
}

public class CreateGroupCommand : ICommand
{
    public string GroupId { get; init; }
}

public class ExitCommand : ICommand
{
}