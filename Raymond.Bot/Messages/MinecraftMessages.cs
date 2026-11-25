using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Bot.Services;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Messages;

public static class MinecraftMessages
{
    private const string AuthorImg =
        "https://cdn.discordapp.com/attachments/1129734051016740886/1534733572664070395/image.png?ex=6a753385&is=6a73e205&hm=19e2c78dc6c2694649dbf8498c1621cb1b6d378f0638da993b7bcd6860e7b00a&";

    public static E ServerInfo(MinecraftStatus status)
    {
        var embed = CreateBaseEmbed(status.Info, DiscordColor.DarkGreen, status.Players);

        embed.Fields =
        [
            new EField("Status", status.Online ? "🟢 Online" : "🔴 Offline", true),
            new EField("Players", $"{status.Players.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length} / {status.Info.MaxPlayers}", true),
            new EField("IP", $"{status.Info.Ip}", true),
        ];

        if (status.Info.Mods is not null)
        {
            embed.Fields.Add(new EField("Mods", status.Info.Mods));
        }

        return embed;
    }

    public static E CommandOutput(MinecraftCommandResult result)
        => CreateBaseEmbed(result.Info, DiscordColor.DarkGreen, result.Output);

    public static E StartPending(MinecraftInfo info)
        => CreateBaseEmbed(info, DiscordColor.Yellow, "Starting server...");

    public static E StartResult(MinecraftInfo info, bool started)
        => CreateBaseEmbed(
            info,
            started ? DiscordColor.DarkGreen : DiscordColor.DarkRed,
            started ? "Server started successfully" : "Failed to start server");

    public static E StopPending(MinecraftInfo info)
        => CreateBaseEmbed(info, DiscordColor.Yellow, "Shutting down server...");

    public static E StopResult(MinecraftInfo info, bool stopped)
        => CreateBaseEmbed(
            info,
            stopped ? DiscordColor.DarkGreen : DiscordColor.DarkRed,
            stopped ? "Server is offline" : "Fail: Server is still online");

    private static E CreateBaseEmbed(MinecraftInfo info, DiscordColor color, string desc) => new()
    {
        Author = "Minecraft",
        AuthorImg = AuthorImg,
        Color = color,
        Title = info.Name,
        Thumbnail = info.ImageUrl,
        Desc = desc,
        Timestamp = true,
    };
}
