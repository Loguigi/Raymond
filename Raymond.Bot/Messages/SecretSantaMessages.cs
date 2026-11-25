using System.Globalization;
using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Shared.Entities;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Messages;

public static class SecretSantaMessages
{
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
                    new EField("Price Limit", santaConfig.PriceLimit?.ToString("C2", CultureInfo.InvariantCulture) ?? "None", true)
                ],
                Desc =
                    $"Welcome to this year's Secret Santa!\n\nTo participate, type `/santa participate`. Once the participation deadline has past **({santaConfig.ParticipationDeadline:dddd, MMMM d})** or an admin locks in the names, you can type `/santa whoigot` to see the name of the person you are gifting.\n\nUse `/santa view` to see these gift exchange details again."
            },
        };
    }
}