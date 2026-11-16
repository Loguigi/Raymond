using DSharpPlus.Entities;

namespace Raymond.Shared.Entities;

public class SantaConfig(DiscordGuild guild)
{
    public DiscordGuild Guild { get; } = guild;
    public DiscordRole? SantaRole { get; set; }
    public DateOnly ParticipationDeadline { get; set; }
    public DateOnly ExchangeDate { get; set; }
    public string ExchangeLocation { get; set; } = string.Empty;
    public string? ExchangeAddress { get; set;  }
    public double? PriceLimit { get; set; }
    public bool LockedIn { get; set; }

    public bool EventHasStarted(DateOnly todayLocal) => ParticipationDeadline > todayLocal || LockedIn;
}