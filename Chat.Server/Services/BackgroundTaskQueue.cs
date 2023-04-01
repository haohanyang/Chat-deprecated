using System.Threading.Channels;

namespace Chat.Server.Services;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, IDatabaseTask> workItem);

    ValueTask<Func<CancellationToken, IDatabaseTask>> DequeueAsync(
        CancellationToken cancellationToken);
}

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, IDatabaseTask>> _queue;

    public BackgroundTaskQueue(int capacity)
    {
        BoundedChannelOptions options = new(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, IDatabaseTask>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, IDatabaseTask> workItem)
    {
        if (workItem is null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }
        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, IDatabaseTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        var workItem =
            await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}