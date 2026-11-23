using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Services;

public class ServerConfigService(IServerConfigRepository configRepo)
{
    public async Task<Result<ServerConfig>> UpdateConfigAsync(
        DiscordGuild guild,
        Func<ServerConfig, string?> mutate,
        CancellationToken ct = default)
    {
        var config = await configRepo.GetConfigAsync(guild.Id.ToDbId(), ct);
        if (config is null)
        {
            return Result<ServerConfig>.Failed("Server config not found");
        }

        var error = mutate(config);
        if (error is not null)
        {
            return Result<ServerConfig>.Failed(error);
        }

        await configRepo.SaveConfigAsync(config, ct);

        return Result<ServerConfig>.Success(config);
    }
}
