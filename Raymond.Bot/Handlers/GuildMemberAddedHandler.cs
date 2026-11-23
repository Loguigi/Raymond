using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Handlers;

public class GuildMemberAddedHandler(
    DiscordClient client,
    ILogger<GuildMemberAddedHandler> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    IConfiguration config)
    : BaseHandler(client, config, logger), IEventHandler<GuildMemberAddedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient s, GuildMemberAddedEventArgs e)
    {
        if (e.Member.IsBot) return;
        
        await HandleWelcomeMessage(e);
    }

    private async Task HandleWelcomeMessage(GuildMemberAddedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var serverConfig = await configRepo.GetConfigAsync(e.Guild.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            
            if (string.IsNullOrWhiteSpace(serverConfig.WelcomeMessage)) return;
            
            var newMember = new ServerMember
            {
                UserId = e.Member.Id.ToDbId(),
                GuildId = e.Guild.Id.ToDbId(),
                Active = true
            };
            
            await memberRepo.SaveMemberAsync(newMember);

            var embed = new E
            {
                Desc = $"### ➡️ {serverConfig.WelcomeMessage} {newMember.DisplayMention()}",
                Color = DiscordColor.Green,
                Timestamp = true
            };

            await Send(await e.Guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), embed);
        }, "leave-message");
    }
}