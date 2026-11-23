using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Services;

public class ServerMemberService(IServerMemberRepository memberRepo)
{
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
