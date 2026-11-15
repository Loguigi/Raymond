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
        List<Type> taskTypes;

        using (var scope = scopeFactory.CreateScope())
        {
            taskTypes =
            [
                .. scope.ServiceProvider
                    .GetServices<IScheduledTask>()
                    .Select(task => task.GetType())
            ];
        }

        var runningTasks = taskTypes.Select(async taskType =>
        {
            logger.LogInformation("Starting task scheduler for {Name}", taskType.Name);

            try
            {
                await StartTaskSchedule(taskType, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Task scheduler for {Name} stopped", taskType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Task scheduler for {Name} failed", taskType.Name);
            }
        });

        await Task.WhenAll(runningTasks);
    }

    private async Task StartTaskSchedule(Type taskType, CancellationToken stoppingToken)
    {
        var runCounter = 0;
        
        string taskName;
        string cronExpression;
            
        using (var scope = scopeFactory.CreateScope())
        {
            var task = (IScheduledTask)scope.ServiceProvider.GetRequiredService(taskType);

            taskName = task.Name;
            cronExpression = task.CronExpression;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nowUtc = DateTimeOffset.UtcNow;

                if (!CronScheduleCalculator.TryGetNextRunUtc(cronExpression, nowUtc, _timeZone, out var nextRunUtc) || !nextRunUtc.HasValue)
                {
                    logger.LogError("Invalid cron expression {CronExpression} for task '{TaskName}'", cronExpression, taskName);
                    return;
                }

                var delay = nextRunUtc.Value - nowUtc;
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }

                logger.LogInformation("Task '{TaskName}' scheduled to run at {NextRun}", taskName, nextRunUtc.Value);

                await Task.Delay(delay, stoppingToken);

                runCounter++;

                logger.LogInformation("Starting task run #{RunCount} for task '{TaskName}'", runCounter, taskName);
                
                using (var scope = scopeFactory.CreateScope())
                {
                    var task = (IScheduledTask)scope.ServiceProvider.GetRequiredService(taskType);

                    await task.ExecuteTaskRun(stoppingToken);
                }

                logger.LogInformation("Completed task run #{RunCount} for task '{TaskName}'", runCounter, taskName);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Task scheduler for '{TaskName}' is stopping", taskName);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing task '{TaskName}'", taskName);
            }
        }
    }
}