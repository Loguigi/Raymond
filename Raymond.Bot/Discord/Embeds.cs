using DSharpPlus.Entities;

namespace Raymond.Bot.Discord;

public class E
{
    public string? Author { get; set; }
    public string? AuthorImg { get; set; }
    public string? Title { get; set; }
    public string? Desc { get; set; }
    public string? Thumbnail { get; set; }
    public string? Img { get; set; }
    public string? Footer { get; set; }
    public string? FooterImg { get; set; }
    public bool Timestamp { get; set; } = false;
    public DiscordColor Color { get; set; } = DiscordColor.Black;
    public List<EField> Fields { get; set; } = [];

    public DiscordEmbed Build()
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle(Title ?? string.Empty)
            .WithDescription(Desc ?? string.Empty)
            .WithAuthor(Author ?? string.Empty, string.Empty, AuthorImg ?? string.Empty)
            .WithColor(Color)
            .WithThumbnail(Thumbnail ?? string.Empty)
            .WithImageUrl(Img ?? string.Empty);

        if (Timestamp)
        {
            embed.WithTimestamp(DateTime.Now);
        }
        else
        {
            embed.WithFooter(Footer ?? string.Empty, FooterImg ?? string.Empty);
        }

        foreach (var field in Fields)
        {
            embed.AddField(field.Name, field.Value, field.Inline);
        }

        return embed.Build();
    }
}

public record EField(string Name, string Value, bool Inline = false);

public static class Embeds
{
    public static E Error(string message) => new()
    {
        Author = "❌ Error",
        Desc = message,
        Color = DiscordColor.Red
    };

    public static E Success(string message) => new()
    {
        Author = "✅ Success",
        Desc = message,
        Color = DiscordColor.Green
    };

    public static E ErrorDetails(Exception ex, DiscordUser? user, DiscordGuild? guild, string? id = null) => new()
    {
        Author = "❌ Error",
        Desc = $"### Message\n{ex.Message}\n### Stack Trace\n{ex.StackTrace}",
        Color = DiscordColor.Red,
        Fields =
        [
            new EField("User", user?.Username ?? "---", true),
            new EField("Guild", guild?.Name ?? "---", true),
            new EField("Time", DateTime.Now.ToString("G"), true),
            new EField("Interaction ID", id ?? "---")
        ]
    };

    public static E ConfigToggle(DiscordGuild guild, string configName, bool toggle) => new()
    {
        Author = guild.Name,
        AuthorImg = guild.IconUrl,
        Title = "Config Change",
        Desc = $"### `{configName}` ➡️ `{(toggle ? "🟢 Enabled" : "🔴 Disabled")}`",
        Color = DiscordColor.Purple,
        Timestamp = true
    };

    public static E ConfigChannel(DiscordGuild guild, string configName, DiscordChannel? channel = null) => new()
    {
        Author = guild.Name,
        AuthorImg = guild.IconUrl,
        Title = "Config Change",
        Desc = $"### `{configName}` ➡️ {channel?.Mention ?? "`🔴 Disabled`"}",
        Color = DiscordColor.Purple,
        Timestamp = true
    };

    public static E ConfigMessage(DiscordGuild guild, string configName, string? oldMsg, string? newMsg) => new()
    {
        Author = guild.Name,
        AuthorImg = guild.IconUrl,
        Title = "Config Change",
        Desc = $"## `{configName}`",
        Color = DiscordColor.Purple,
        Timestamp = true,
        Fields = 
        [
            new EField("Before", oldMsg ?? "`🔴 Disabled`"),
            new EField("After", newMsg ?? "`🔴 Disabled`")
        ]
    };
}