namespace Raymond.Bot.Scheduling;

public class SchedulingOptions
{
    public const string SectionName = "Scheduling";
    
    public required string Timezone { get; set; }
    public required string DiscordSync { get; set; }
    public required string Birthdays { get; set; }
    public required string GuildEvents { get; set; }
}