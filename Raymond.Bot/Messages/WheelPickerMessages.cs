using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Bot.Utilities;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Messages;

public static class WheelPickerMessages
{
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
}