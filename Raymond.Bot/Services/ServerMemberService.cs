using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Services;

public class ServerMemberService(IServerMemberRepository memberRepo)
{
    public Task<ServerMember> GetMemberByUserAsync(
        DiscordGuild guild,
        DiscordUser user,
        CancellationToken ct = default)
        => memberRepo.GetMemberAsync(guild.Id.ToDbId(), user.Id.ToDbId(), ct);

    public async Task<ServerMember> SetNameAsync(
        DiscordGuild guild,
        DiscordUser user,
        string name,
        CancellationToken ct = default)
    {
        var member = await memberRepo.GetMemberAsync(guild.Id.ToDbId(), user.Id.ToDbId(), ct);

        member.Name = name;
        await memberRepo.SaveMemberAsync(member, ct);

        return member;
    }

    public async Task<ServerMember> SetBirthdayAsync(
        DiscordGuild guild,
        DiscordUser user,
        DateOnly birthday,
        CancellationToken ct = default)
    {
        var member = await memberRepo.GetMemberAsync(guild.Id.ToDbId(), user.Id.ToDbId(), ct);
        
        member.Birthday = birthday;
        await memberRepo.SaveMemberAsync(member, ct);
        
        return member;
    }
}
