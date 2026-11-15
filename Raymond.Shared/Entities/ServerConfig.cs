using DSharpPlus.Entities;

namespace Raymond.Shared.Entities;


public class ServerConfig(DiscordGuild guild, DiscordChannel defaultChannel)
{
    public DiscordGuild Guild { get; } = guild;
    public string? WelcomeMessage { get; set; }
    public string? LeaveMessage { get; set; }
    public DiscordChannel DefaultChannel { get; set; } = defaultChannel;
    public bool BirthdayNotifications { get; set; } = true;
    public int BirthdayTimerHour { get; set; } = 1;
    public string BirthdayMessage { get; set; } = "Happy Birthday!";
    public bool EnableLevels { get; set; }
    public string LevelUpMessage { get; set; } = "has leveled up!";
    public DiscordChannel? LoggingChannel { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    public long GuildId => (long)Guild.Id;
    public long DefaultChannelId => (long)DefaultChannel.Id;
    public long? LoggingChannelId => LoggingChannel is null ? null : (long)LoggingChannel.Id;
}