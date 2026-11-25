using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Domain;

namespace Raymond.Bot.Handlers;

public class MessageCreatedHandler(
    DiscordClient client,
    ILogger<MessageCreatedHandler> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    LevelCalculator levelCalculator,
    IConfiguration config)
    : BaseHandler(client, config, logger), IEventHandler<MessageCreatedEventArgs>
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
            // Check config for enabled levels
            var serverConfig = await configRepo.GetConfigAsync(e.Guild.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            if (!serverConfig.EnableLevels) return;
            
            // Get member data and grant experience
            var member = await memberRepo.GetMemberAsync(e.Guild.Id.ToDbId(), e.Author.Id.ToDbId());
            var levelUp = levelCalculator.GrantExp(member);
            await memberRepo.SaveMemberAsync(member);

            // Display level up card if user levelled up
            if (levelUp)
            {
                var embed = new E
                {
                    Desc = $"### ⬆️ {e.Author.Mention} {serverConfig.LevelUpMessage}",
                    Footer = $"Level {member.Level - 1} ➡️ Level {member.Level}",
                    Color = DiscordColor.Aquamarine
                };
                
                await Send(await e.Guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), embed);
            }
        }, "exp_gain");
    }
}