using DSharpPlus;
using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Shared.Entities;
using Raymond.Shared.Models;
using Raymond.Shared.Services;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Utilities;

public static class CustomMessages
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

    public static M BirthdayList(DiscordGuild guild, BirthdayService birthdayService, List<ServerMember> members, DateOnly localDate, BirthdayListId id)
    {
        var paginator = new Paginator<ServerMember>(
            members.Where(m => m.Birthday.HasValue).OrderBy(m => birthdayService.GetNextBirthday(m, localDate)), 10, id.PageToken);
        
        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Title = "Birthdays",
                Desc = paginator.RenderPage((member, _) =>
                {
                    var nextBirthday = birthdayService.GetNextBirthday(member, localDate);
                    var age = birthdayService.GetAge(member, localDate);
                    
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

    public static M WheelSpin(ServerMember member, DiscordUser user, WheelPicker wheel, WheelSpinId id)
    {
        var spinId = (id with { RemoveOption = false }).Build();
        var spinAndRemoveId = (id with { RemoveOption = true }).Build();
        
        return new M
        {
            Embed = new E
            {
                Author = member.DisplayName(user.Username),
                AuthorImg = user.AvatarUrl,
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

    public static M WheelOptionList(WheelPicker wheel, DiscordGuild guild, WheelOptionListId id)
    {
        var paginator = new Paginator<WheelOption>(wheel.Options.OrderBy(o => o.TempRemoved).ThenBy(o => o.Option, StringComparer.InvariantCultureIgnoreCase), 15, id.PageToken);

        return new M
        {
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
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

    public static M SantaEventDetails(DiscordGuild guild, TimeZoneInfo timeZone, SantaConfig santaConfig, bool firstSend)
    {
        return new M
        {
            Content = firstSend ? "@everyone" : string.Empty,
            EveryoneMention = firstSend,
            Embed = new E
            {
                Author = guild.Name,
                AuthorImg = guild.IconUrl,
                Color = DiscordColor.SapGreen,
                Title = $"{TimeZoneHelper.GetCurrentDate(timeZone).Year} Secret Santa",
                Fields =
                [
                    new EField("Date", $"{santaConfig.ExchangeDate:dddd, MMMM d}", true),
                    new EField("Location", santaConfig.ExchangeLocation, true),
                    new EField("Price Limit", santaConfig.PriceLimit?.ToString("C2") ?? "None", true)
                ],
                Desc =
                    $"Welcome to this year's Secret Santa!\n\nTo participate, type `/santa participate`. Once the participation deadline has past **({santaConfig.ParticipationDeadline:dddd, MMMM d})** or an admin locks in the names, you can type `/santa whoigot` to see the name of the person you are gifting.\n\nUse `/santa view` to see these gift exchange details again."
            },
        };
    }
}