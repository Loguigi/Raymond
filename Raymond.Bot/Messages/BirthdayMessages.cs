using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Bot.Utilities;
using Raymond.Shared.Domain;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Messages;

public static class BirthdayMessages
{
    public static M BirthdayList(DiscordGuild guild, BirthdayRules birthdayRules, List<ServerMember> members, DateOnly localDate, BirthdayListId id)
    {
        var paginator = new Paginator<ServerMember>(
            members.Where(m => m.Birthday.HasValue).OrderBy(m => birthdayRules.GetNextBirthday(m, localDate)), 10, id.PageToken);
        
        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Birthdays",
                Desc = paginator.RenderPage((member, _) =>
                {
                    var nextBirthday = birthdayRules.GetNextBirthday(member, localDate);
                    var age = birthdayRules.GetAge(member, localDate);
                    
                    var turningAge = nextBirthday == localDate
                        ? age
                        : age + 1;
                    
                    return $"* {member.DisplayMention()}: {nextBirthday.ToDiscordFormat()} `({age} => {turningAge})`";
                }),
                Color = DiscordColor.HotPink,
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new BirthdayListId(page).Build()),
        };
    }
}