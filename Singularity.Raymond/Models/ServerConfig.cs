using DSharpPlus.Entities;

namespace Singularity.Raymond.Models;

public class ServerConfig(DiscordGuild guild)
{
    public DiscordGuild Guild { get; } = guild;
    public string? WelcomeMessage { get; set; }
    public string? LeaveMessage { get; set; }
    public DiscordChannel DefaultChannel { get; set; } = guild.GetDefaultChannel()!;
    public bool BirthdayNotifications { get; set; } = true;
    public int BirthdayTimerHour { get; set; } = 1;
    public string BirthdayMessage { get; set; } = "Happy Birthday!";
    public bool EnableLevels { get; set; }
    public string LevelUpMessage { get; set; } = "has leveled up!";
    public DiscordChannel? LoggingChannel { get; set; }
}