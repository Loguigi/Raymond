using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;

namespace Raymond.Bot.Handlers;

public class GuildMemberRemovedHandler(
    DiscordClient client,
    ILogger<GuildMemberRemovedHandler> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    IConfigurationRoot config)
    : BaseHandler(client, config, logger), IEventHandler<GuildMemberRemovedEventArgs>
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
            var member = await memberRepo.GetMemberAsync(e.Guild.Id.ToDbId(), e.Member.Id.ToDbId());
            member.Active = false;
            
            await memberRepo.SaveMemberAsync(member);
        }, "inactive-member");
    }

    private async Task HandleLeaveMessage(GuildMemberRemovedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var serverConfig = await configRepo.GetConfigAsync(e.Guild.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            
            if (string.IsNullOrWhiteSpace(serverConfig.LeaveMessage)) return;
            
            var member = await memberRepo.GetMemberAsync(e.Guild.Id.ToDbId(), e.Member.Id.ToDbId());

            var embed = new E
            {
                Desc = $"### ⬅️ {member.DisplayMention()} {serverConfig.LeaveMessage}",
                Color = DiscordColor.Red,
                Timestamp = true
            };

            await Send(await e.Guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), embed);
        }, "leave-message");
    }
}