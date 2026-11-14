using Cronos;

namespace Raymond.Bot.Scheduling;

public static class CronScheduleCalculator
{
    public static bool TryGetNextRunUtc(
        string cronSchedule,
        DateTimeOffset fromUtc,
        TimeZoneInfo timeZone,
        out DateTimeOffset? nextRunUtc)
    {
        nextRunUtc = null;

        if (string.IsNullOrWhiteSpace(cronSchedule))
        {
            return false;
        }

        if (!CronExpression.TryParse(cronSchedule.Trim(), out var expression))
        {
            return false;
        }

        var nextLocal = expression.GetNextOccurrence(fromUtc, timeZone, false);
        if (nextLocal is not { })
        {
            return false;
        }

        nextRunUtc = nextLocal.Value.ToUniversalTime();
        return true;
    }
}