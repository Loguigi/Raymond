using DSharpPlus.Entities;
using Raymond.Shared.Entities;

namespace Raymond.Data.Repositories;

public interface ISecretSantaRepository
{
    Task<List<SantaParticipant>> GetSantaParticipantsAsync(DiscordGuild guild, CancellationToken ct = default);
    Task SaveSantaParticipantAsync(SantaParticipant participants, CancellationToken ct = default);
    Task<SantaConfig?> GetSantaConfigAsync(DiscordGuild guild, CancellationToken ct = default);
    Task SaveSantaConfigAsync(SantaConfig config, CancellationToken ct = default);
    Task SaveCoupleAsync(SantaParticipant first, SantaParticipant second, CancellationToken ct = default);
}

public class SqlSecretSantaRepository
{
    
}