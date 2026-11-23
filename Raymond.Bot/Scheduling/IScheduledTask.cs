namespace Raymond.Bot.Scheduling;

public interface IScheduledTask
{
    string Name { get; }
    string CronExpression { get; }
    bool RunOnStartup { get; }
    Task ExecuteTaskRun(CancellationToken taskRunCt);
}