using DSharpPlus;
using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Bot.Utilities;
using Raymond.Shared.Entities;
using Raymond.Shared.Models;

namespace Raymond.Bot.Messages;

public static class MiscMessages
{
    public static M Identities(DiscordGuild guild, DiscordClient client, List<ServerMember> members, IdentitiesId id)
    {
        var paginator = new Paginator<ServerMember>(
            members.OrderByDescending(m => m.Name, StringComparer.InvariantCultureIgnoreCase), 10, id.PageToken);

        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Super Goober List",
                Color = DiscordColor.Azure,
                Desc = paginator.RenderPage((member, _) => $"* {member.Mention()} ➡️ **{member.Name ?? "`?`"}**"),
                Footer = paginator.PageStatus
            },
            Buttons = PaginationButtons.Create(paginator, page => new IdentitiesId(page).Build())
        };
    }

    public static M CoinFlip(ServerMember member, DiscordUser user, CoinFlipId id)
    {
        return new M
        {
            Embed = new E
            {
                Author = member.DisplayName(user.Username),
                AuthorImg = user.AvatarUrl,
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