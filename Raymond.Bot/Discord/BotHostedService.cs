using DSharpPlus;
using Microsoft.Extensions.Hosting;

namespace Raymond.Bot.Discord;

public sealed class BotHostedService(DiscordClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await client.ConnectAsync();
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.DisconnectAsync();
        await base.StopAsync(cancellationToken);
    }
}