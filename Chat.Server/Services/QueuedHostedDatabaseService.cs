namespace Chat.Server.Services;

public sealed class QueuedHostedDatabaseService : BackgroundService
{
    private readonly ILogger<QueuedHostedDatabaseService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedHostedDatabaseService(
        IServiceProvider services,
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedDatabaseService> logger)
    {
        (_taskQueue, _logger, Services) = (taskQueue, logger, services);
    }

    public IServiceProvider Services { get; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{nameof(QueuedHostedDatabaseService)} is running.{Environment.NewLine}" +
            $"{Environment.NewLine}Tap W to add a work item to the " +
            $"background queue.{Environment.NewLine}");

        return ProcessTaskQueueAsync(stoppingToken);
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        using (var scope = Services.CreateScope())
        {
            var scopedDatabaseService = scope.ServiceProvider.GetService<IDatabaseService>();
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var workItem =
                        await _taskQueue.DequeueAsync(stoppingToken);

                    var task = workItem(stoppingToken);
                    await scopedDatabaseService.ProcessTask(task);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task work item.");
                }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{nameof(QueuedHostedDatabaseService)} is stopping.");

        await base.StopAsync(stoppingToken);
    }
}