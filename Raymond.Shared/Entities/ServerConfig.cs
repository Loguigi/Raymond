namespace Raymond.Shared.Entities;

public class ServerConfig
{
    public long GuildId { get; set; }
    public string? WelcomeMessage { get; set; }
    public string? LeaveMessage { get; set; }
    public long DefaultChannelId { get; set; }
    public bool BirthdayNotifications { get; set; } = true;
    public int BirthdayTimerHour { get; set; } = 1;
    public string BirthdayMessage { get; set; } = "Happy Birthday!";
    public bool EnableLevels { get; set; }
    public string LevelUpMessage { get; set; } = "has leveled up!";
    public long? LoggingChannelId { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
}