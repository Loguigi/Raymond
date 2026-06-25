using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Repositories;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Handlers;

public class GuildMemberRemovedHandler(
    DiscordClient client,
    ILogger<GuildMemberRemovedHandler> logger,
    ServerMemberRepository memberRepo,
    ServerConfigRepository configRepo)
    : BaseHandler(client, logger), IEventHandler<GuildMemberRemovedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient s, GuildMemberRemovedEventArgs e)
    {
        if (e.Member.IsBot) return;
        
        await HandleInactiveMember(e);
        await HandleLeaveMessage(e);
    }

    private async Task HandleInactiveMember(GuildMemberRemovedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var member = await memberRepo.GetMemberAsync(e.Guild, e.Member);
            member.Active = false;
            
            await memberRepo.SaveMemberAsync(member);
        }, "inactive-member");
    }

    private async Task HandleLeaveMessage(GuildMemberRemovedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var config = await configRepo.GetConfigAsync(e.Guild);
            if (string.IsNullOrWhiteSpace(config.LeaveMessage)) return;
            
            var member = await memberRepo.GetMemberAsync(e.Guild, e.Member);

            var embed = new E
            {
                Desc = $"### ⬅️ {member.DisplayMention} {config.LeaveMessage}",
                Color = DiscordColor.Red,
                Timestamp = true
            };

            await Send(config.DefaultChannel, embed);
        }, "leave-message");
    }
}