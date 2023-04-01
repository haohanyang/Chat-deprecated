using Chat.Common;

namespace Chat.Server.Services;

public interface IDatabaseTask
{
    public CancellationToken CancellationToken { get; init; }
}

public class AddGroupTask : IDatabaseTask
{
    public string GroupId { get; init; }
    public CancellationToken CancellationToken { get; init; }
}


public class RemoveGroupTask : IDatabaseTask
{
    public string GroupId { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

public class AddMemberTask : IDatabaseTask
{
    public string MemberId { get; init; }
    public string GroupId { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

public class RemoveMemberTask : IDatabaseTask
{
    public string MemberId { get; init; }
    public string GroupId { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

public class AddUserMessageTask : IDatabaseTask
{
    public Message Message { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

public class AddGroupMessageTask : IDatabaseTask
{
    public Message Message { get; init; }
    public CancellationToken CancellationToken { get; init; }
}