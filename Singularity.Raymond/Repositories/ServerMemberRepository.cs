using Dapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Models;
using Singularity.Shared.Data;

namespace Singularity.Raymond.Repositories;

public sealed class ServerMemberRepository(
    IDbConnectionFactory factory,
    DiscordClient client,
    ILogger<ServerMemberRepository> logger) 
    : BaseRepository(logger, factory)
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

    public async Task<ServerMember?> GetMemberAsync(DiscordGuild guild, DiscordUser user)
    {
        if (user.IsBot) return null;

        var member = new ServerMember(user, guild);
        
        var memberRecord = await GetFirstAsync<ServerMemberRecord>("GetMember", 
            new DynamicParameters(new { GuildId = (long)guild.Id, UserId = (long)user.Id }));

        if (memberRecord is null)
        {
            await SaveMember(member);
            
            memberRecord = await GetFirstAsync<ServerMemberRecord>("GetServerMember", 
                new DynamicParameters(new { GuildId = (long)guild.Id, UserId = (long)user.Id }));
        }

        member.Id = memberRecord!.Id;
        member.Name = memberRecord.Name;
        member.Level = memberRecord.Level;
        member.Experience = memberRecord.Experience;
        member.Birthday = memberRecord.Birthday;
        member.Active = memberRecord.Active;

        return member;
    }

    public async Task<List<ServerMember>> GetMembersAsync(DiscordGuild guild)
    {
        var serverMembers = new List<ServerMember>();
        var memberRecords = await GetAllAsync<ServerMemberRecord>("GetServerMembers", new DynamicParameters(new { GuildId = (long)guild.Id }));
        var memberRecordsList = memberRecords.ToList();
        
        // Map all existing user records
        foreach (var member in memberRecordsList)
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
                await SaveMember(member);
            }
            else
            {
                var newMember = new ServerMember(guildMember, guild)
                {
                    Active = true
                };
                
                await SaveMember(newMember);
                
                newMember = await GetMemberAsync(guild, guildMember);
                if (newMember is not null) serverMembers.Add(newMember);
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
    
    public async Task SaveMember(ServerMember member)
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
    
    public async Task<bool> GrantExp(ServerMember member)
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
        await SaveMember(member);
        
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