using Dapper;
using Microsoft.Extensions.Logging;
using Raymond.Data.Sql;
using Raymond.Shared.Entities;

namespace Raymond.Data.Repositories;

public interface ISecretSantaRepository
{
    Task<SantaParticipant?> GetParticipantAsync(long guildId, long userId, CancellationToken ct = default);
    Task<List<SantaParticipant>> GetSantaParticipantsAsync(long guildId, CancellationToken ct = default);
    Task SaveParticipantAsync(SantaParticipant participants, CancellationToken ct = default);
    Task<SantaConfig?> GetSantaConfigAsync(long guildId, CancellationToken ct = default);
    Task SaveSantaConfigAsync(SantaConfig config, CancellationToken ct = default);
    Task DeleteSantaConfigAsync(SantaConfig config, CancellationToken ct = default);
    Task SaveCoupleAsync(SantaParticipant first, SantaParticipant second, CancellationToken ct = default);
}

public sealed class SqlSecretSantaRepository(
    IDbConnectionFactory factory,
    ILogger<SqlSecretSantaRepository> logger)
    : BaseSqlRepository(logger, factory), ISecretSantaRepository
{
    private sealed class SantaConfigRecord
    {
        public long GuildId { get; set; }
        public long SantaRoleId { get; set; }
        public DateOnly ParticipationDeadline { get; set; }
        public DateOnly ExchangeDate { get; set; }
        public string ExchangeLocation { get; set; } = string.Empty;
        public string? ExchangeAddress { get; set; }
        public double? PriceLimit { get; set; }
        public bool LockedIn { get; set; }
    }

    private sealed class SantaParticipantRecord
    {
        public int MemberId { get; set; }
        public long UserId { get; set; }
        public int? GifteeMemberId { get; set; }
        public long? GifteeUserId { get; set; }
        public int? SignificantOtherMemberId { get; set; }
        public long? SignificantOtherUserId { get; set; }
        public bool GiftReady { get; set; }
    }

    public async Task<SantaParticipant?> GetParticipantAsync(long guildId, long userId, CancellationToken ct = default)
    {
        var record = await GetFirstAsync<SantaParticipantRecord>("GetSantaParticipant",
            new { GuildId = guildId, UserId = userId });
        if (record is null) return null;

        return await ToParticipantAsync(guildId, record);
    }

    public async Task<List<SantaParticipant>> GetSantaParticipantsAsync(long guildId, CancellationToken ct = default)
    {
        var records = await GetAllAsync<SantaParticipantRecord>("GetSantaParticipants", new { GuildId = guildId });
        
        var participants = new List<SantaParticipant>(records.Count);
        foreach (var record in records)
        {
            participants.Add(await ToParticipantAsync(guildId, record));
        }
        
        return participants;
    }

    public async Task SaveParticipantAsync(SantaParticipant participant, CancellationToken ct = default)
    {
        await ExecuteAsync("SaveSantaParticipant", new
        {
            GuildId = participant.Member.GuildId,
            MemberId = participant.Member.Id,
            GifteeMemberId = participant.Giftee?.Id,
            SignificantOtherMemberId = participant.SignificantOther?.Id,
            GiftReady = participant.GiftReady
        });
    }

    public async Task<SantaConfig?> GetSantaConfigAsync(long guildId, CancellationToken ct = default)
    {
        var configRecord = await GetFirstAsync<SantaConfigRecord>("GetSantaConfig", new { GuildId = guildId });
        if (configRecord is null) return null;

        return new SantaConfig
        {
            GuildId = configRecord.GuildId,
            SantaRoleId = configRecord.SantaRoleId,
            ParticipationDeadline = configRecord.ParticipationDeadline,
            ExchangeDate = configRecord.ExchangeDate,
            ExchangeLocation = configRecord.ExchangeLocation,
            ExchangeAddress = configRecord.ExchangeAddress,
            PriceLimit = configRecord.PriceLimit,
            LockedIn = configRecord.LockedIn
        };
    }

    public async Task SaveSantaConfigAsync(SantaConfig config, CancellationToken ct = default)
    {
        var record = new SantaConfigRecord
        {
            GuildId = config.GuildId,
            SantaRoleId = config.SantaRoleId,
            ParticipationDeadline = config.ParticipationDeadline,
            ExchangeDate = config.ExchangeDate,
            ExchangeLocation = config.ExchangeLocation,
            ExchangeAddress = config.ExchangeAddress,
            PriceLimit = config.PriceLimit,
            LockedIn = config.LockedIn
        };
        
        await ExecuteAsync("SaveSantaConfig", record);
    }

    public async Task DeleteSantaConfigAsync(SantaConfig config, CancellationToken ct = default)
    {
        await ExecuteAsync("DeleteSantaConfig", new { GuildId = config.GuildId });
    }

    public async Task SaveCoupleAsync(SantaParticipant first, SantaParticipant second, CancellationToken ct = default)
    {
        await ExecuteAsync("SaveSantaCouple", new
        {
            GuildId = first.Member.GuildId,
            FirstMemberId = first.Member.Id,
            SecondMemberId = second.Member.Id
        });
    }

    private async Task<SantaParticipant> ToParticipantAsync(long guildId, SantaParticipantRecord record)
    {
        var member = await GetFirstAsync<ServerMemberRecord>("GetMember",
            new { GuildId = guildId, UserId = record.UserId });

        var giftee = record.GifteeUserId is { } gifteeUserId
            ? await GetFirstAsync<ServerMemberRecord>("GetMember", new { GuildId = guildId, UserId = gifteeUserId })
            : null;

        var significantOther = record.SignificantOtherUserId is { } soUserId
            ? await GetFirstAsync<ServerMemberRecord>("GetMember", new { GuildId = guildId, UserId = soUserId })
            : null;

        return new SantaParticipant(ToServerMember(guildId, member!))
        {
            Giftee = giftee is null ? null : ToServerMember(guildId, giftee),
            SignificantOther = significantOther is null ? null : ToServerMember(guildId, significantOther),
            GiftReady = record.GiftReady
        };
    }

    private sealed class ServerMemberRecord
    {
        public int Id { get; set; }
        public long GuildId { get; set; }
        public long UserId { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public DateOnly? Birthday { get; set; }
        public bool Active { get; set; }
    }

    private static ServerMember ToServerMember(long guildId, ServerMemberRecord record) => new()
    {
        Id = record.Id,
        GuildId = guildId,
        UserId = record.UserId,
        Name = record.Name,
        Level = record.Level,
        Experience = record.Experience,
        Birthday = record.Birthday,
        Active = record.Active
    };
}
