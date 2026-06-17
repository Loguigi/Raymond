using Dapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Models;
using Singularity.Shared.Data;

namespace Singularity.Raymond.Repositories;

public class ServerMemberRepository(
    IDbConnectionFactory factory,
    DiscordClient client,
    ILogger<ServerMemberRepository> logger) 
    : BaseRepository(logger, factory)
{
    private const int ExpGain = 1;

    public async Task<ServerMember?> GetMemberAsync(DiscordGuild guild, DiscordUser user)
    {
        if (user.IsBot) return null;
        
        var member = await GetFirstAsync<ServerMember>("GetServerMember", 
            new DynamicParameters(new { GuildId = (long)guild.Id, UserId = (long)user.Id }));

        if (member is null)
        {
            member = new ServerMember
            {
                UserId = (long)user.Id,
                GuildId = (long)guild.Id
            };

            await SaveMember(member);
        }

        member.Guild = guild;
        member.User = user;

        return member;
    }

    public async Task<List<ServerMember>> GetMembersAsync(DiscordGuild guild)
    {
        var members = await GetAllAsync<ServerMember>("GetServerMembers", new DynamicParameters(new { GuildId = (long)guild.Id }));
        var serverMembers = members.ToList();
        
        foreach (var member in serverMembers)
        {
            member.Guild = guild;
            member.User = await GetDiscordUserAsync(member.UserId);
        }

        foreach (var guildMember in guild.Members.Values)
        {
            if (guildMember.IsBot) continue;
            
            var member = serverMembers.FirstOrDefault(x => x.UserId == (long)guildMember.Id);

            if (member is not null)
            {
                if (member.Active) continue;

                member.Active = true;
                await SaveMember(member);
            }
            else
            {
                var newMember = new ServerMember
                {
                    UserId = (long)guildMember.Id,
                    GuildId = (long)guild.Id,
                    Active = true
                };
                
                await SaveMember(newMember);
            }
        }
        
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
        await ExecuteAsync("SaveServerMember", new DynamicParameters(member));
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