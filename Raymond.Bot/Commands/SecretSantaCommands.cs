using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Bot.Services;
using Raymond.Bot.Utilities;
using Raymond.Data.Repositories;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Commands;

[Command("santa"), Description("Commands for managing a Secret Santa"), RequireGuild]
public class SecretSantaCommands(
    SecretSantaService santaService,
    IServerConfigRepository configRepo,
    IConfiguration config,
    DiscordClient client,
    ILogger<SecretSantaCommands> logger)
    : BaseCommand(client, config, logger)
{
    public enum WinterMonths
    {
        [ChoiceDisplayName("January")]
        January,
        [ChoiceDisplayName("November")]
        November,
        [ChoiceDisplayName("December")]
        December
    }
    
    [Command("start"), Description("Start the Secret Santa")]
    public async Task StartSecretSanta(
        SlashCommandContext ctx,
        [Description("Month for the participation deadline")]
        WinterMonths participationMonth,
        [Description("Day for the participation deadline"), MinMaxValue(1, 31)]
        int participationDay,
        [Description("Month for the gift exchange")]
        WinterMonths exchangeMonth,
        [Description("Day for the gift exchange"), MinMaxValue(1, 31)]
        int exchangeDay,
        [Description("Location for the gift exchange"), MinMaxValue(1, 30)]
        string exchangeLocation,
        [Description("Price limit to set for gifts (optional)")]
        double? priceLimit = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            var timeZone = serverConfig.TimeZone;
            
            if (!WinterDateHelper.TryGetWinterDate(timeZone, GetMonth(participationMonth), participationDay, out var participationDeadlineDate))
            {
                await Send(ctx, Embeds.Error("Invalid participation deadline date"), true);
                return;
            }

            if (!WinterDateHelper.TryGetWinterDate(timeZone, GetMonth(exchangeMonth), exchangeDay, out var giftExchangeDate))
            {
                await Send(ctx, Embeds.Error("Invalid gift exchange date"), true);
                return;
            }

            var result = await santaService.StartSecretSantaAsync(ctx.Guild!, participationDeadlineDate, giftExchangeDate, exchangeLocation, priceLimit);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, CustomMessages.SantaEventDetails(ctx.Guild!, serverConfig.TimeZone, result.Body, true));
        }, "santa start");
    }

    [Command("view"), Description("View the Secret Santa event details")]
    public async Task ViewEventDetails(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);

            var result = await santaService.GetSantaConfigAsync(ctx.Guild!);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }
            
            await Send(ctx, CustomMessages.SantaEventDetails(ctx.Guild!, serverConfig.TimeZone, result.Body, false), true);
        }, "santa view");
    }

    [Command("couple"), Description("Save 2 participants as a couple so they don't get each other")]
    public async Task SaveCouple(SlashCommandContext ctx, DiscordMember first, DiscordMember second)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, first) || await IsBot(ctx, second)) return;
            
            var result = await santaService.SaveCoupleAsync(ctx.Guild!, first, second);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }
            
            await Send(ctx, Embeds.Success(result.Message));

        }, "santa couple");
    }

    private static int GetMonth(WinterMonths winterMonth) => winterMonth switch
    {
        WinterMonths.January => 1,
        WinterMonths.November => 11,
        WinterMonths.December => 12,
        _ => throw new ArgumentOutOfRangeException(nameof(winterMonth), winterMonth, null)
    };
}