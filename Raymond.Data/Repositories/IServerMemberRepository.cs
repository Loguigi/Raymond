using Dapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Raymond.Data.Models;
using Raymond.Data.Sql;
using Raymond.Shared.Entities;

namespace Raymond.Data.Repositories;

public interface IServerMemberRepository
{
    Task<ServerMember> GetMemberAsync(DiscordGuild guild, DiscordUser user, CancellationToken ct = default);
    Task<List<ServerMember>> GetMembersAsync(DiscordGuild guild,  CancellationToken ct = default);
    Task SaveMemberAsync(ServerMember member,  CancellationToken ct = default);
    Task<bool> GrantExp(ServerMember member,  CancellationToken ct = default);
    ServerMember GetNextBirthday(List<ServerMember> members);

}

public sealed class SqlServerMemberRepository(
    IDbConnectionFactory factory,
    DiscordClient client,
    ILogger<SqlServerMemberRepository> logger) 
    : BaseSqlRepository(logger, factory), IServerMemberRepository
{
    private const int ExpGain = 1;

    private sealed class ServerMemberRecord
    {
        public int Id { get; set; }
        public long GuildId { get; set; }
        public long UserId { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Active { get; set; }
    }

    public async Task<ServerMember> GetMemberAsync(DiscordGuild guild, DiscordUser user, CancellationToken ct = default)
    {
        if (user.IsBot) throw new Exception("Bot passed in as user");

        var member = new ServerMember(user, guild);
        
        var memberRecord = await GetFirstAsync<ServerMemberRecord>("GetMember", 
            new { GuildId = (long)guild.Id, UserId = (long)user.Id });

        if (memberRecord is null)
        {
            await SaveMemberAsync(member, ct).ConfigureAwait(false);
            
            memberRecord = await GetFirstAsync<ServerMemberRecord>("GetMember", 
                new { GuildId = (long)guild.Id, UserId = (long)user.Id });
        }

        member.Id = memberRecord!.Id;
        member.Name = memberRecord.Name;
        member.Level = memberRecord.Level;
        member.Experience = memberRecord.Experience;
        member.Birthday = memberRecord.Birthday;
        member.Active = memberRecord.Active;

        return member;
    }

    public async Task<List<ServerMember>> GetMembersAsync(DiscordGuild guild, CancellationToken ct = default)
    {
        var serverMembers = new List<ServerMember>();
        var memberRecords = await GetAllAsync<ServerMemberRecord>("GetServerMembers", new { GuildId = (long)guild.Id });
        
        // Map all existing user records
        foreach (var member in memberRecords)
        {
            serverMembers.Add(new ServerMember(await GetDiscordUserAsync(member.UserId), guild)
            {
                Id = member.Id,
                Name = member.Name,
                Level = member.Level,
                Experience = member.Experience,
                Birthday = member.Birthday,
                Active = member.Active
            });
        }

        // Create entries for server users that aren't in SQL
        foreach (var guildMember in guild.Members.Values)
        {
            if (guildMember.IsBot) continue;
            
            var member = serverMembers.FirstOrDefault(x => x.User == guildMember);

            if (member is not null)
            {
                if (member.Active) continue;

                member.Active = true;
                await SaveMemberAsync(member, ct).ConfigureAwait(false);
            }
            else
            {
                var newMember = new ServerMember(guildMember, guild)
                {
                    Active = true
                };
                
                await SaveMemberAsync(newMember, ct).ConfigureAwait(false);
                
                newMember = await GetMemberAsync(guild, guildMember, ct).ConfigureAwait(false);
                serverMembers.Add(newMember);
            }
        }
        
        // Calculate level rankings
        serverMembers = serverMembers
            .Where(x => x.Active)
            .OrderByDescending(x => x.Level)
            .ThenByDescending(y => y.Experience).ToList();

        for (var i = 0; i < serverMembers.Count; i++)
        {
            serverMembers[i].Rank = i + 1;
        }

        logger.LogDebug("Found {Count} members in guild {Guild}", serverMembers.Count, guild.Name);
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
    
    public async Task<bool> GrantExp(ServerMember member, CancellationToken ct = default)
    {
        bool leveledUp = false;

        member.Experience += ExpGain;
        int newLevel = (int)Math.Sqrt((double)member.Experience / 2 + 1);
        if (newLevel > member.Level) 
        {
            member.Level++;
            leveledUp = true;
            logger.LogDebug("Member {UserId} leveled up to {Level}", member.UserId, member.Level);
        }
        await SaveMemberAsync(member);
        
        return leveledUp;
    }
    
    public ServerMember GetNextBirthday(List<ServerMember> members)
    {
        members = members.Where(x => x.NextBirthday.HasValue).OrderBy(x => x.NextBirthday).ToList();
        var nextBirthday = members.First();
        logger.LogDebug("Next birthday is {User} on {Date}", nextBirthday.UserId, nextBirthday.NextBirthday);
        return nextBirthday;
    }
    
    private async Task<DiscordUser> GetDiscordUserAsync(long userId) => await client.GetUserAsync((ulong)userId);
}