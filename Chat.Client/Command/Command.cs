using Chat.Common;

namespace Chat.Client.Command;

public interface ICommand
{
}

public class SendMessageCommand : ICommand
{
    public string Receiver { get; set; }
    public ReceiverType ReceiverType { get; set; }
    public string Message { get; set; }
}

public class JoinGroupCommand : ICommand
{
    public string GroupId { get; set; }
}

public class LeaveGroupCommand : ICommand
{
    public string GroupId { get; set; }
}

public class CreateGroupCommand : ICommand
{
    public string GroupId { get; set; }
}

public class ExitCommand : ICommand
{
}