using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Bot.Utilities;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Messages;

public static class LevelMessages
{
    public static M Leaderboard(DiscordGuild guild, SortedDictionary<int, ServerMember> rankings, LeaderboardId id)
    {
        var paginator = new Paginator<KeyValuePair<int, ServerMember>>(rankings, 10, id.PageToken);
        
        return new M
        {   
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Leaderboard",
                Desc = paginator.RenderPage((member, _) =>
                {
                    var rankText = member.Key switch
                    {
                        1 => "### 🥇",
                        2 => "### 🥈",
                        3 => "### 🥉",
                        _ => $"### **__#{member.Key}__**"
                    };

                    return $"{rankText} {member.Value.DisplayMention()} `LVL {member.Value.Level}` `{member.Value.Experience} XP`";
                }),
                Color = DiscordColor.Orange,
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new LeaderboardId(page).Build())
        };
    }
}