namespace Raymond.Bot.Scheduling;

public interface IScheduledTask
{
    string Name { get; }
    string CronExpression { get; }
    Task ExecuteTaskRun(CancellationToken taskRunCt);
}