using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Domain;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Services;

public record MinecraftStatus(MinecraftInfo Info, string Players, bool Online);

public record MinecraftCommandResult(MinecraftInfo Info, string Output);

public class MinecraftService(
    IMinecraftRepository mcRepo,
    MinecraftRules minecraftRules,
    IConfiguration config)
{
    public bool IsAdmin(DiscordUser user) => user.Id == ulong.Parse(config["AdminId"] ?? "0");

    public Task<MinecraftInfo> GetInfoAsync(DiscordGuild guild, CancellationToken ct = default)
        => mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);

    public async Task<MinecraftStatus> GetStatusAsync(DiscordGuild guild, CancellationToken ct = default)
    {
        var info = await mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);
        var players = await mcRepo.GetPlayerListAsync(info, ct);

        return new MinecraftStatus(info, players, await mcRepo.IsServerOnline(info, ct));
    }

    public async Task<bool> IsOnlineAsync(DiscordGuild guild, CancellationToken ct = default)
    {
        var info = await mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);
        return await mcRepo.IsServerOnline(info, ct);
    }

    public async Task<Result<MinecraftInfo>> SetupAsync(
        DiscordGuild guild,
        string? container = null,
        string? ip = null,
        string? name = null,
        string? image = null,
        int? maxPlayers = null,
        string? mods = null,
        CancellationToken ct = default)
    {
        var info = await mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);
        minecraftRules.ApplySetup(info, container, ip, name, image, maxPlayers, mods);

        await mcRepo.SaveInfoAsync(info, ct);

        return Result<MinecraftInfo>.Success(info, "Saved server info");
    }

    public async Task<Result<MinecraftCommandResult>> RunCommandAsync(
        DiscordGuild guild,
        string command,
        CancellationToken ct = default)
    {
        var info = await mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);

        if (!await mcRepo.IsServerOnline(info, ct))
        {
            return Result<MinecraftCommandResult>.Failed("Server is offline");
        }

        var output = await mcRepo.ExecuteCommandAsync(info, command, ct);

        return Result<MinecraftCommandResult>.Success(new MinecraftCommandResult(info, output));
    }

    public async Task<Result<bool>> StartAsync(DiscordGuild guild, CancellationToken ct = default)
    {
        var info = await mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);

        await mcRepo.StartServerAsync(info, ct);
        await Task.Delay(TimeSpan.FromSeconds(10), ct);

        return await mcRepo.IsServerOnline(info, ct)
            ? Result<bool>.Success(true, "Server started successfully")
            : Result<bool>.Failed("Failed to start server");
    }

    public async Task<Result<bool>> StopAsync(DiscordGuild guild, CancellationToken ct = default)
    {
        var info = await mcRepo.GetInfoAsync(guild.Id.ToDbId(), ct);

        await mcRepo.StopServerAsync(info, ct);
        await Task.Delay(TimeSpan.FromSeconds(15), ct);

        return await mcRepo.IsServerOnline(info, ct)
            ? Result<bool>.Failed("Fail: Server is still online")
            : Result<bool>.Success(true, "Server is offline");
    }
}
