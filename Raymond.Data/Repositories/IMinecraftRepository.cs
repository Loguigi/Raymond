using Microsoft.Extensions.Logging;
using Raymond.Data.Sql;
using Raymond.Shared.Entities;
using Raymond.Shared.Services;

namespace Raymond.Data.Repositories;

public interface IMinecraftRepository
{
    Task<MinecraftInfo> GetInfoAsync(long guildId, CancellationToken ct = default);
    Task SaveInfoAsync(MinecraftInfo info, CancellationToken ct = default);
    Task<bool> IsServerOnline(MinecraftInfo info, CancellationToken ct = default);
    Task<bool> StartServerAsync(MinecraftInfo info, CancellationToken ct = default);
    Task<bool> StopServerAsync(MinecraftInfo info, CancellationToken ct = default);
    Task<string> GetPlayerListAsync(MinecraftInfo info, CancellationToken ct = default);
    Task<string> ExecuteCommandAsync(MinecraftInfo info, string command, CancellationToken ct = default);
}

public class SqlMinecraftRepository(
    DockerService docker, 
    IDbConnectionFactory factory, 
    ILogger<SqlMinecraftRepository> logger)
    : BaseSqlRepository(logger, factory), IMinecraftRepository
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

    public async Task<MinecraftInfo> GetInfoAsync(long guildId, CancellationToken ct = default)
    {
        var infoRecord = await GetFirstAsync<MinecraftInfoRecord>("GetMinecraftInfo", new { GuildId = guildId });

        if (infoRecord is null) return new MinecraftInfo();
        
        return new MinecraftInfo
        {
            GuildId = guildId,
            ContainerName = infoRecord.ContainerName,
            Ip = infoRecord.Ip,
            Name = infoRecord.Name,
            ImageUrl = infoRecord.ImageUrl,
            MaxPlayers = infoRecord.MaxPlayers,
            Mods = infoRecord.Mods
        };
    }

    public async Task SaveInfoAsync(MinecraftInfo info, CancellationToken ct = default)
    {
        var record = new MinecraftInfoRecord
        {
            GuildId = info.GuildId,
            ContainerName = info.ContainerName,
            Ip = info.Ip,
            Name = info.Name,
            ImageUrl = info.ImageUrl,
            MaxPlayers = info.MaxPlayers,
            Mods = info.Mods
        };
        
        await ExecuteAsync("SaveMinecraftInfo", record);
    }

    public async Task<bool> IsServerOnline(MinecraftInfo info, CancellationToken ct = default)
    {
        return await docker.IsContainerOnline(info.ContainerName);
    }

    public async Task<bool> StartServerAsync(MinecraftInfo info, CancellationToken ct = default)
    {
        if (await docker.IsContainerOnline(info.ContainerName)) return false;
        
        return await docker.TryStartContainerAsync(info.ContainerName);
    }

    public async Task<bool> StopServerAsync(MinecraftInfo info, CancellationToken ct = default)
    {
        if (!await docker.IsContainerOnline(info.ContainerName)) return false;
        
        for (var i = 10; i > 0; i--)
        {
            await ExecuteMinecraftCommand(info, $"say Server is shutting down in {i} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        
        await ExecuteMinecraftCommand(info, "say GOODBYE");
        await ExecuteMinecraftCommand(info, "stop");

        return true;
    }

    public async Task<string> GetPlayerListAsync(MinecraftInfo info, CancellationToken ct = default)
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

    public async Task<string> ExecuteCommandAsync(MinecraftInfo info, string command, CancellationToken ct = default)
    {
        return await ExecuteMinecraftCommand(info, command);
    }

    private async Task<string> ExecuteMinecraftCommand(MinecraftInfo info, string command)
    {
        if (!(await docker.GetContainerStatusAsync(info.ContainerName)).Equals("running", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return (await docker.TryExecuteAsync(info.ContainerName, $"rcon-cli {command}"))
            .Replace("\e[0m", string.Empty).Replace("[0m", string.Empty);
    }
}