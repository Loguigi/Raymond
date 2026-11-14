using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Raymond.Bot.Scheduling;

public class TaskSchedulerService(
    IServiceScopeFactory scopeFactory,
    ILogger<TaskSchedulerService> logger,
    IOptions<SchedulingOptions> options)
    : BackgroundService
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.Timezone);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var tasks = scope.ServiceProvider.GetServices<IScheduledTask>();

        foreach (var task in tasks)
        {
            try
            {
                using var taskCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                logger.LogInformation("Starting task schedule {Name} at {Now}", task.GetType().Name, DateTimeOffset.UtcNow);

                await StartTaskSchedule(task, taskCts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting task {Name}", task.GetType().Name);
            }
        }
    }

    private async Task StartTaskSchedule(IScheduledTask task, CancellationTokenSource taskCts)
    {
        int runCounter = 0;
        
        while (!taskCts.IsCancellationRequested)
        {
            runCounter++;
            
            try
            {
                var nowUtc = DateTimeOffset.UtcNow;
                if (!CronScheduleCalculator.TryGetNextRunUtc(task.CronExpression, nowUtc, _timeZone, out var nextRunUtc) || !nextRunUtc.HasValue)
                {
                    logger.LogError("Invalid cron expression {CronExpression} for task '{TaskName}'", task.CronExpression, task.Name);
                    await taskCts.CancelAsync();
                    return;
                }
                
                var delay = nextRunUtc.Value - nowUtc;
                await Task.Delay(delay.Milliseconds, taskCts.Token);
                
                logger.LogInformation("");
                await task.ExecuteTaskRun(taskCts.Token);
            }
            catch (OperationCanceledException) when (taskCts.IsCancellationRequested)
            {
                logger.LogWarning("Task run #{RunCount} for task '{TaskName}' cancelled by host", runCounter, task.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing task run #{RunCount} for task '{Name}'", runCounter, task.Name);
            }
        }
    }
}