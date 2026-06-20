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
        var paginator = new Paginator<ServerMember>(
            members.Where(m => m.Birthday.HasValue).OrderBy(m => m.NextBirthday), 10, id.PageToken);
        
        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Birthdays",
                Desc = paginator.RenderPage((member, _) =>
                {
                    var turningAge = member.NextBirthday?.Date == DateTime.Today
                        ? member.Age
                        : member.Age + 1;
                    
                    return $"* {member.DisplayMention}: {member.FormattedNextBirthday} `({turningAge})`";
                }),
                Color = DiscordColor.HotPink,
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new BirthdayListId(page).Build()),
        };
    }

    public static M Identities(DiscordGuild guild, List<ServerMember> members, IdentitiesId id)
    {
        var paginator = new Paginator<ServerMember>(
            members.OrderByDescending(m => m.Name), 10, id.PageToken);

        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Super Goober List",
                Color = DiscordColor.Azure,
                Desc = paginator.RenderPage((member, _) => $"* {member.User.Mention} ➡️ **{member.Name ?? "`?`"}**"),
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new IdentitiesId(page).Build())
        };
    }

    public static M CoinFlip(ServerMember member, CoinFlipId id)
    {
        return new M
        {
            Embed = new E
            {
                Author = member.DisplayName,
                AuthorImg = member.User.AvatarUrl,
                Desc = $"# {id.LastFlip}",
                Timestamp = true,
                Color = id.LastFlip == FlipResult.Heads ? DiscordColor.Green : DiscordColor.Red,
                Fields =
                [
                    new EField("Heads", id.HeadCount.ToString(), true),
                    new EField("Tails", id.TailsCount.ToString(), true)
                ]
            },
            Buttons = 
            [
                new Button(DiscordButtonStyle.Success, id.Build(), "Flip Again", "🪙")
            ]
        };
    }
}