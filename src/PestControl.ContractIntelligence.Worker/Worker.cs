namespace PestControl.ContractIntelligence.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private static readonly Action<ILogger, DateTimeOffset, Exception?> WorkerRunning =
        LoggerMessage.Define<DateTimeOffset>(
            LogLevel.Information,
            new EventId(1, nameof(WorkerRunning)),
            "Worker running at: {Timestamp}");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                WorkerRunning(logger, DateTimeOffset.UtcNow, null);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
