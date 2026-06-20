using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Repositories;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Handlers;

public class MessageCreatedHandler(
    DiscordClient client,
    ILogger<MessageCreatedHandler> logger,
    ServerMemberRepository memberRepo)
    : BaseHandler(client, logger), IEventHandler<MessageCreatedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient s, MessageCreatedEventArgs e)
    {
        await HandleExpGain(e);
    }

    private async Task HandleExpGain(MessageCreatedEventArgs e)
    {
        if (e.Author.IsBot) return;

        await ExecuteAsync(e, async () =>
        {
            var member = await memberRepo.GetMemberAsync(e.Guild, e.Author);
            // TODO: config check

            var levelUp = await memberRepo.GrantExp(member);

            if (levelUp)
            {
                var embed = new E
                {
                    Desc = $"### ⬆️ {member.User.Mention} has levelled up!",
                    Footer = $"Level {member.Level - 1} ➡️ Level {member.Level}",
                    Color = DiscordColor.Aquamarine
                };
                // TODO: config default channel
                var defaultChannel = e.Guild.GetDefaultChannel();
                
                await Send(defaultChannel!, embed);
            }
        }, "exp_gain");
    }
}