using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Raymond.Data.Repositories;
using Singularity.Discord;

namespace Raymond.Bot.Handlers;

public class GuildMemberAddedHandler(
    DiscordClient client,
    ILogger<GuildMemberAddedHandler> logger,
    ServerMemberRepository memberRepo,
    ServerConfigRepository configRepo)
    : BaseHandler(client, logger), IEventHandler<GuildMemberAddedEventArgs>
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
            var config = await configRepo.GetConfigAsync(e.Guild);
            if (string.IsNullOrWhiteSpace(config.WelcomeMessage)) return;
            
            var member = await memberRepo.GetMemberAsync(e.Guild, e.Member);

            var embed = new E
            {
                Desc = $"### ➡️ {config.WelcomeMessage} {member.DisplayMention}",
                Color = DiscordColor.Green,
                Timestamp = true
            };

            await Send(config.DefaultChannel, embed);
        }, "leave-message");
    }
}