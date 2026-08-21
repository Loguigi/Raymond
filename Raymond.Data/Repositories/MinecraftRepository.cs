using Artifice.Data;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Raymond.Data.Models;
using Raymond.Data.Services;

namespace Raymond.Data.Repositories;

public class MinecraftRepository(
    DockerService docker, 
    IDbConnectionFactory factory, 
    ILogger<MinecraftRepository> logger)
    : BaseRepository(logger, factory)
{
    private sealed class MinecraftInfoRecord
    {
        public long GuildId { get; set; }
        public string ContainerName { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int MaxPlayers { get; set; }
        public string? Mods { get; set; }
    }

    public async Task<MinecraftInfo> GetInfoAsync(DiscordGuild guild)
    {
        var infoRecord = await GetFirstAsync<MinecraftInfoRecord>("GetMinecraftInfo", new { GuildId = (long)guild.Id });
        
        if (infoRecord is null) return new MinecraftInfo(guild);
        
        return new MinecraftInfo(guild)
        {
            ContainerName = infoRecord.ContainerName,
            Ip = infoRecord.Ip,
            Name = infoRecord.Name,
            ImageUrl = infoRecord.ImageUrl,
            MaxPlayers = infoRecord.MaxPlayers,
            Mods = infoRecord.Mods
        };
    }

    public async Task SaveInfoAsync(MinecraftInfo info)
    {
        var record = new MinecraftInfoRecord
        {
            GuildId = (long)info.Guild.Id,
            ContainerName = info.ContainerName,
            Ip = info.Ip,
            Name = info.Name,
            ImageUrl = info.ImageUrl,
            MaxPlayers = info.MaxPlayers,
            Mods = info.Mods
        };
        
        await ExecuteAsync("SaveMinecraftInfo", record);
    }
    
    public async Task<string> GetServerStatusAsync(MinecraftInfo info)
    {
        return await docker.GetContainerStatusAsync(info.ContainerName);
    }

    public async Task<bool> StartServerAsync(MinecraftInfo info)
    {
        return await docker.TryStartContainerAsync(info.ContainerName);
    }

    public async Task<bool> StopServerAsync(MinecraftInfo info)
    {
        var status = await GetServerStatusAsync(info);
        if (status != "running") return false;
        
        for (var i = 10; i > 0; i--)
        {
            await ExecuteMinecraftCommand(info, $"say Server is shutting down in {i} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        
        await ExecuteMinecraftCommand(info, "say GOODBYE");
        await ExecuteMinecraftCommand(info, "stop");

        return true;
    }

    public async Task<string> GetPlayerListAsync(MinecraftInfo info)
    {
        var output = await ExecuteMinecraftCommand(info, "list");
        int separatorIndex = output.IndexOf(':');

        return separatorIndex < 0
            ? string.Empty
            : string.Join(
                "\n",
                output[(separatorIndex + 1)..]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(name => $"* `{name}`"));
    }

    public async Task<string> ExecuteCommandAsync(MinecraftInfo info, string command)
    {
        return await ExecuteMinecraftCommand(info, command);
    }

    private async Task<string> ExecuteMinecraftCommand(MinecraftInfo info, string command)
    {
        return (await docker.TryExecuteAsync(info.ContainerName, $"rcon-cli {command}"))
            .Replace("\e[0m", string.Empty).Replace("[0m", string.Empty);
    }
}