using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Discord;

public static class DiscordExtensions
{
    public static long ToDbId(this ulong discordId)
    {
        return checked((long)discordId);
    }

    public static ulong ToDiscordId(this long dbId)
    {
        return checked((ulong)dbId);
    }

    public static string ToDiscordFormat(this DateOnly? date)
    {
        return date?.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture) ?? "N/A";
    }

    public static string DisplayMention(this ServerMember member)
    {
        return member.Name ?? UserMention(member.UserId);
    }

    public static string Mention(this ServerMember member)
    {
        return UserMention(member.UserId);
    }
    
    public static string DisplayName(this ServerMember member, string username)
    {
        return member.Name ?? username;
    }
    
    public static async Task<DiscordUser> GetDiscordUser(this ServerMember member, DiscordClient client)
    {
        return await client.GetUserAsync(member.UserId.ToDiscordId());
    }
    
    private static string UserMention(long discordId)
    {
        return $"<@{discordId}>";
    }
}