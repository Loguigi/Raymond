using Dapper;
using Microsoft.Extensions.Logging;
using Raymond.Data.Sql;
using Raymond.Shared.Entities;

namespace Raymond.Data.Repositories;

public interface IServerMemberRepository
{
    Task<ServerMember> GetMemberAsync(long guildId, long userId, CancellationToken ct = default);
    Task<List<ServerMember>> GetMembersAsync(long guildId, CancellationToken ct = default);
    Task SaveMemberAsync(ServerMember member, CancellationToken ct = default);
}

public sealed class SqlServerMemberRepository(
    IDbConnectionFactory factory,
    ILogger<SqlServerMemberRepository> logger) 
    : BaseSqlRepository(logger, factory), IServerMemberRepository
{
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

    public async Task<ServerMember> GetMemberAsync(long guildId, long userId, CancellationToken ct = default)
    {
        var member = new ServerMember
        {
            GuildId = guildId,
            UserId = userId
        };
        
        var memberRecord = await GetFirstAsync<ServerMemberRecord>("GetMember", 
            new { GuildId = guildId, UserId = userId });

        if (memberRecord is null)
        {
            await SaveMemberAsync(member, ct).ConfigureAwait(false);
            
            memberRecord = await GetFirstAsync<ServerMemberRecord>("GetMember", 
                new { GuildId = guildId, UserId = userId });
        }

        member.Id = memberRecord!.Id;
        member.Name = memberRecord.Name;
        member.Level = memberRecord.Level;
        member.Experience = memberRecord.Experience;
        member.Birthday = memberRecord.Birthday;
        member.Active = memberRecord.Active;

        return member;
    }

    public async Task<List<ServerMember>> GetMembersAsync(long guildId, CancellationToken ct = default)
    {
        var serverMembers = new List<ServerMember>();
        var memberRecords = await GetAllAsync<ServerMemberRecord>("GetServerMembers", new { GuildId = guildId });
        
        // Map all existing user records
        foreach (var member in memberRecords)
        {
            serverMembers.Add(new ServerMember
            {
                Id = member.Id,
                UserId = member.UserId,
                GuildId = guildId,
                Name = member.Name,
                Level = member.Level,
                Experience = member.Experience,
                Birthday = member.Birthday,
                Active = member.Active
            });
        }

        return serverMembers;
    }
    
    public async Task SaveMemberAsync(ServerMember member, CancellationToken ct = default)
    {
        var record = new ServerMemberRecord
        {
            Id = member.Id,
            GuildId = member.GuildId,
            UserId = member.UserId,
            Name = member.Name,
            Level = member.Level,
            Experience = member.Experience,
            Birthday = member.Birthday,
            Active = member.Active,
        };
        
        await ExecuteAsync("SaveServerMember", new DynamicParameters(record));
    }
}