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
            var member = await memberRepo.GetMemberAsync(e.Guild, e.Creator);
            var serverConfig = await configRepo.GetConfigAsync(e.Guild);
            var url = $"https://discord.com/events/{e.Guild.Id}/{e.Event.Id}";

            var m = new M
            {
                Content = url,
                Embed = new E
                {
                    Desc = $"❗️New event created by {member.DisplayMention}",
                    Color = DiscordColor.Magenta,
                }
            };
                        
            await Send(serverConfig.DefaultChannel, m);
        }, "event_created");
    }
}