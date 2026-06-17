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
                Color = DiscordColor.Orange,
            },
            Buttons = PaginationButtons.Create(paginator, page => id.Build())
        };
    }
}