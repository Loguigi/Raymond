namespace Raymond.Shared.Entities;

public class SantaConfig
{
    public long GuildId { get; set; }
    public long SantaRoleId { get; set; }
    public DateOnly ParticipationDeadline { get; set; }
    public DateOnly ExchangeDate { get; set; }
    public string ExchangeLocation { get; set; } = string.Empty;
    public string? ExchangeAddress { get; set;  }
    public double? PriceLimit { get; set; }
    public bool LockedIn { get; set; }

    public bool EventHasStarted(DateOnly todayLocal) => ParticipationDeadline > todayLocal || LockedIn;
}