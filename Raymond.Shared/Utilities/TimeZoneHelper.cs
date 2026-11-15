
namespace Raymond.Shared.Utilities;

public static class TimeZoneHelper
{
    public static DateTimeOffset GetCurrentTime(TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
    }

    public static DateOnly GetCurrentDate(TimeZoneInfo timeZone)
    {
        return DateOnly.FromDateTime(GetCurrentTime(timeZone).DateTime);
    }
    
    public static DateTimeOffset ConvertToLocalTime(DateTimeOffset utcDateTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTime(utcDateTime, timeZone);
    }
}