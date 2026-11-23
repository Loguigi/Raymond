using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;

namespace Raymond.Bot.Handlers;

public class ScheduledGuildEventHandler(
    DiscordClient client,
    ILogger<ScheduledGuildEventHandler> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    IConfiguration config)
    : BaseHandler(client, config, logger), IEventHandler<ScheduledGuildEventCreatedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient s, ScheduledGuildEventCreatedEventArgs e)
    {
        await HandleEventCreation(e);
    }

    private async Task HandleEventCreation(ScheduledGuildEventCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var member = await memberRepo.GetMemberAsync(e.Guild.Id.ToDbId(), e.Creator.Id.ToDbId());
            var serverConfig = await configRepo.GetConfigAsync(e.Guild.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            var url = $"https://discord.com/events/{e.Guild.Id}/{e.Event.Id}";

            var m = new M
            {
                Content = url,
                Embed = new E
                {
                    Desc = $"❗️New event created by {member.DisplayMention()}",
                    Color = DiscordColor.Magenta,
                }
            };
                        
            await Send(await e.Guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), m);
        }, "event_created");
    }
}