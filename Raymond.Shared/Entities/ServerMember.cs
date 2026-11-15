using DSharpPlus.Entities;

namespace Raymond.Shared.Entities;

public class ServerMember(DiscordUser user, DiscordGuild guild)
{
    public int Id { get; set; } = 0;
    public DiscordUser User { get; } = user;
    public DiscordGuild Guild { get; } = guild;
    public string? Name { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public DateOnly? Birthday { get; set; }
    public bool Active { get; set; } = true;
    public int Rank { get; set; } = 0;
    
    public long UserId => (long)User.Id;
    public long GuildId => (long)Guild.Id;
    public string DisplayName => Name ?? User.Username;
    public string DisplayMention => Name ?? User.Mention;
    public int ExpNeededForNextLevel => 2 * (int)Math.Pow(Level + 1, 2) - 2;
    
    public string GetFormattedBirthday(DateOnly? birthday) => birthday?.ToString("MMMM d, yyyy") ?? "N/A";

    public int GetAge(DateOnly todayLocal)
    {
        if (Birthday == null) return 0;
            
        var age = todayLocal.Year - Birthday.Value.Year;

        if (todayLocal.Month == Birthday.Value.Month) 
        {
            // birthday is within month
            if (todayLocal.Day < Birthday.Value.Day) 
            {
                // birthday hasn't occured yet
                age--;
            }
        } 
        else if (todayLocal.Month < Birthday.Value.Month) 
        {
            // birthday month not reached yet
            age--;
        }

        return age;
    }

    public DateOnly? GetNextBirthday(DateOnly todayLocal)
    {
        if (!Birthday.HasValue) return null;
        
        var next = new DateOnly(todayLocal.Year, Birthday.Value.Month, Birthday.Value.Day);

        if (next < todayLocal)
            next = next.AddYears(1);

        return next;
    }
    
    public bool HasBirthdayToday(DateOnly todayLocal) => GetNextBirthday(todayLocal) is { } next && next == todayLocal;

    public int GetDaysUntilBirthday(DateOnly todayLocal)
    {
        var nextBirthday = GetNextBirthday(todayLocal);
        if (!nextBirthday.HasValue) return 0;
        
        return nextBirthday.Value.DayNumber - todayLocal.DayNumber;
    }
}