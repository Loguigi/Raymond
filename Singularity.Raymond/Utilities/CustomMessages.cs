using DSharpPlus.Entities;
using Singularity.Raymond.Models;
using Singularity.Shared.Discord;
using Singularity.Shared.Utilities;

namespace Singularity.Raymond.Utilities;

public static class CustomMessages
{
    public static M Leaderboard(DiscordGuild guild, List<ServerMember> members, LeaderboardId id)
    {
        var paginator = new Paginator<ServerMember>(members, 10, id.PageToken);
        
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
}