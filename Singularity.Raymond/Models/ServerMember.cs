using DSharpPlus.Entities;

namespace Singularity.Raymond.Models;

public class ServerMember(DiscordUser user, DiscordGuild guild)
{
    public int Id { get; set; } = 0;
    public DiscordUser User { get; } = user;
    public DiscordGuild Guild { get; } = guild;
    public string? Name { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public DateTime? Birthday { get; set; }
    public bool Active { get; set; } = true;
    public int Rank { get; set; } = 0;
    
    public long UserId => (long)User.Id;
    public long GuildId => (long)Guild.Id;
    public string DisplayName => Name ?? User.Username;
    public string DisplayMention => Name ?? User.Mention;
    public int ExpNeededForNextLevel => 2 * (int)Math.Pow(Level + 1, 2) - 2;
    public DateTime? NextBirthday => Birthday?.AddYears(Age + 1) ?? DateTime.MinValue;
    public string FormattedBirthday => Birthday?.ToString("MMMM d, yyyy") ?? "N/A";
    public string FormattedNextBirthday => NextBirthday?.ToString("MMMM d, yyyy") ?? "N/A";
    public int DaysUntilBirthday => (NextBirthday?.Date - DateTime.Today.Date)?.Days ?? 0;
    
    public int Age 
    {
        get 
        {
            if (Birthday == null) return 0;
            
            var current = DateTime.Now;
            var age = current.Year - Birthday.Value.Year;

            if (current.Month == Birthday.Value.Month) 
            {
                // birthday is within month
                if (current.Day < Birthday.Value.Day) 
                {
                    // birthday hasn't occured yet
                    age--;
                }
            } 
            else if (current.Month < Birthday.Value.Month) 
            {
                // birthday month not reached yet
                age--;
            }

            return age;
        }
    }
    
}