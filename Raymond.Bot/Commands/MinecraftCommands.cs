using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Bot.Messages;
using Raymond.Bot.Services;

namespace Raymond.Bot.Commands;

[Command("mc"), RequireGuild]
public class MinecraftCommands(
    MinecraftService mcService,
    DiscordClient client,
    ILogger<MinecraftCommands> logger,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    [Command("info")]
    public async Task Info(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var status = await mcService.GetStatusAsync(ctx.Guild!);

            await Send(ctx, MinecraftMessages.ServerInfo(status));
        }, "mc info");
    }

    [Command("setup")]
    public async Task Setup(SlashCommandContext ctx,
        string? container = null,
        string? ip = null,
        string? name = null,
        string? image = null,
        int? maxPlayers = null,
        string? mods = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!mcService.IsAdmin(ctx.User)) return;

            var result = await mcService.SetupAsync(ctx.Guild!, container, ip, name, image, maxPlayers, mods);
            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.Success("Saved server info"), true);
        }, "mc setup");
    }

    [Command("command")]
    public async Task Command(SlashCommandContext ctx, string command)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!mcService.IsAdmin(ctx.User))
            {
                await Send(ctx, Embeds.Error("You are not authorized to use this command."), true);
                return;
            }

            var result = await mcService.RunCommandAsync(ctx.Guild!, command);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            if (string.IsNullOrWhiteSpace(result.Body.Output))
            {
                await ctx.RespondAsync("Success", true);
                return;
            }

            await Send(ctx, MinecraftMessages.CommandOutput(result.Body));
        }, "mc command");
    }

    [Command("start")]
    public async Task Start(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var info = await mcService.GetInfoAsync(ctx.Guild!);

            if (await mcService.IsOnlineAsync(ctx.Guild!))
            {
                await Send(ctx, Embeds.Error("Server is already online"), true);
                return;
            }

            await Send(ctx, MinecraftMessages.StartPending(info));
            var result = await mcService.StartAsync(ctx.Guild!);

            await FollowUp(ctx, MinecraftMessages.StartResult(info, result.Succeeded));
        }, "mc start");
    }

    [Command("stop")]
    public async Task Stop(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!mcService.IsAdmin(ctx.User))
            {
                await Send(ctx, Embeds.Error("You are not authorized to use this command."), true);
                return;
            }

            var info = await mcService.GetInfoAsync(ctx.Guild!);

            if (!await mcService.IsOnlineAsync(ctx.Guild!))
            {
                await Send(ctx, Embeds.Error("Server is already offline"), true);
                return;
            }

            await Send(ctx, MinecraftMessages.StopPending(info));
            var result = await mcService.StopAsync(ctx.Guild!);

            await FollowUp(ctx, MinecraftMessages.StopResult(info, result.Succeeded));
        }, "mc stop");
    }
}
