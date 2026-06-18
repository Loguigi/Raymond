using DSharpPlus.Entities;
using Singularity.Raymond.Models;
using Singularity.Shared.Discord;
using Singularity.Shared.Utilities;

namespace Singularity.Raymond.Utilities;

public static class CustomMessages
{
    public static M Leaderboard(DiscordGuild guild, List<ServerMember> members, LeaderboardId id)
    {
        var paginator = new Paginator<ServerMember>(members.OrderBy(m => m.Rank), 10, id.PageToken);
        
        return new M
        {   
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Leaderboard",
                Desc = paginator.RenderPage((member, _) =>
                {
                    var rankText = member.Rank switch
                    {
                        1 => "### 🥇",
                        2 => "### 🥈",
                        3 => "### 🥉",
                        _ => $"### **__#{member.Rank}__**"
                    };

                    return $"{rankText} {member.DisplayMention} `LVL {member.Level}` `{member.Experience} XP`";
                }),
                Color = DiscordColor.Orange,
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new LeaderboardId(page).Build())
        };
    }

    public static M BirthdayList(DiscordGuild guild, List<ServerMember> members, BirthdayListId id)
    {
        var paginator = new Paginator<ServerMember>(members.OrderBy(m => m.NextBirthday), 15, id.PageToken);
        
        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Birthday List",
                Desc = paginator.RenderPage((member, _) => $"* {member.DisplayMention}: {member.FormattedNextBirthday} `({member.Age + 1})`"),
                Color = DiscordColor.HotPink,
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new BirthdayListId(page).Build()),
        };
    }
}