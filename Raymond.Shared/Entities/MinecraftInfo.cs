using DSharpPlus.Entities;

namespace Raymond.Shared.Entities;

public class MinecraftInfo(DiscordGuild guild)
{
    public DiscordGuild Guild { get; } = guild;
    public string ContainerName { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public string? Mods { get; set; }
}