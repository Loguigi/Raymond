using System.Globalization;
using Raymond.Shared.Entities;

namespace Raymond.Shared.Domain;

public class BirthdayRules
{
    public bool TryParseBirthday(long year, long month, long day, out DateOnly birthday)
    {
        if (DateOnly.TryParse($"{year}-{month}-{day}", CultureInfo.InvariantCulture, out var date)
            && date < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            birthday = date;
            return true;
        }

        birthday = default;
        return false;
    }

    public int GetAge(ServerMember member, DateOnly todayLocal)
    {
        if (member.Birthday == null) return 0;
            
        var age = todayLocal.Year - member.Birthday.Value.Year;

        if (todayLocal.Month == member.Birthday.Value.Month) 
        {
            // birthday is within month
            if (todayLocal.Day < member.Birthday.Value.Day) 
            {
                // birthday hasn't occured yet
                age--;
            }
        } 
        else if (todayLocal.Month < member.Birthday.Value.Month) 
        {
            // birthday month not reached yet
            age--;
        }

        return age;
    }

    public DateOnly? GetNextBirthday(ServerMember member, DateOnly todayLocal)
    {
        if (!member.Birthday.HasValue) return null;
        
        var next = new DateOnly(todayLocal.Year, member.Birthday.Value.Month, member.Birthday.Value.Day);

        if (next < todayLocal)
            next = next.AddYears(1);

        return next;
    }
    
    public bool HasBirthdayToday(ServerMember member, DateOnly todayLocal) => GetNextBirthday(member, todayLocal) is { } next && next == todayLocal;

    public int GetDaysUntilBirthday(ServerMember member, DateOnly todayLocal)
    {
        var nextBirthday = GetNextBirthday(member, todayLocal);
        if (!nextBirthday.HasValue) return 0;
        
        return nextBirthday.Value.DayNumber - todayLocal.DayNumber;
    }
    
    public ServerMember? GetNextBirthday(List<ServerMember> members, DateOnly todayLocal)
    {
        if (!members.Any(m => m.Birthday.HasValue)) return null;
        
        members = [.. members.Where(m => GetNextBirthday(m, todayLocal).HasValue).OrderBy(m => GetNextBirthday(m, todayLocal))];
        var nextBirthday = members.First();
        
        return nextBirthday;
    }
}