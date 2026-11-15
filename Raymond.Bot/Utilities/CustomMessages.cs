using DSharpPlus.Entities;
using Raymond.Data.Models;
using Raymond.Bot.Discord;
using Raymond.Shared.Entities;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Utilities;

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

    public static M BirthdayList(DiscordGuild guild, List<ServerMember> members, DateOnly localDate, BirthdayListId id)
    {
        var paginator = new Paginator<ServerMember>(
            members.Where(m => m.Birthday.HasValue).OrderBy(m => m.GetNextBirthday(localDate)), 10, id.PageToken);
        
        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Birthdays",
                Desc = paginator.RenderPage((member, _) =>
                {
                    var nextBirthday = member.GetNextBirthday(localDate);
                    var age = member.GetAge(localDate);
                    
                    var turningAge = nextBirthday == localDate
                        ? age
                        : age + 1;
                    
                    return $"* {member.DisplayMention}: {member.GetFormattedBirthday(nextBirthday)} `({age} => {turningAge})`";
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

    public static M WheelSpin(ServerMember member, WheelPicker wheel, WheelSpinId id)
    {
        var spinId = (id with { RemoveOption = false }).Build();
        var spinAndRemoveId = (id with { RemoveOption = true }).Build();
        
        return new M
        {
            Embed = new E
            {
                Author = member.DisplayName,
                AuthorImg = member.User.AvatarUrl,
                Title = $"🌀 {wheel.Name} 🌀",
                Desc = $"# {id.OptionSpun}",
                Thumbnail = wheel.ImageUrl ?? "",
                Color = DiscordColor.Azure,
                Timestamp = true,
                Fields =
                [
                    new EField("Count", id.SpinCount.ToString(), true),
                    new EField("Remaining / Total", $"{wheel.AvailableOptions.Count} / {wheel.Options.Count}", true),
                    new EField("Previous", id.RemoveOption ? $"❌ ~~{id.LastOptionSpun}~~ ❌" : id.LastOptionSpun, true)
                ]
            },
            Buttons =
            [
                new Button(DiscordButtonStyle.Primary, spinId, "Spin Again", "🔁", wheel.AvailableOptions.Count == 0),
                new Button(DiscordButtonStyle.Danger, spinAndRemoveId, "Spin and Remove", "🗑️", wheel.AvailableOptions.Count == 0)
            ]
        };
    }

    public static M WheelOptionList(WheelPicker wheel, WheelOptionListId id)
    {
        var paginator = new Paginator<WheelOption>(wheel.Options.OrderBy(o => o.TempRemoved).ThenBy(o => o.Option), 15, id.PageToken);

        return new M
        {
            Embed = new E
            {
                Author = wheel.Guild.Name,
                AuthorImg = wheel.Guild.IconUrl,
                Title = $"{wheel.Name} - {wheel.Options.Count} Options",
                Thumbnail = wheel.ImageUrl ?? "",
                Color = DiscordColor.Gold,
                Footer = paginator.PageStatus,
                Desc = paginator.RenderPage((option, index) =>
                    $"{index}. {(option.TempRemoved ? $"~~{option.Option}~~" : option.Option)}")
            },
            Buttons = PaginationButtons.Create(paginator, page => new WheelOptionListId(wheel.Id, page).Build())
        };
    }
}