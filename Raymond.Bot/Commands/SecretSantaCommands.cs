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
using Raymond.Bot.Utilities;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Commands;

[Command("santa"), Description("Commands for managing a Secret Santa"), RequireGuild]
public class SecretSantaCommands(
    ISecretSantaRepository santaRepo,
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
            var santaConfig = await santaRepo.GetSantaConfigAsync(ctx.Guild!);
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!);

            if (santaConfig is not null)
            {
                await Send(ctx, Embeds.Error("Secret Santa event has already started"), true);
                return;
            }

            var timeZone = serverConfig.TimeZone;
            if (!TryGetWinterDate(timeZone, participationMonth, participationDay, out var participationDeadlineDate))
            {
                await Send(ctx, Embeds.Error("Invalid participation deadline date"), true);
                return;
            }

            if (!TryGetWinterDate(timeZone, exchangeMonth, exchangeDay, out var giftExchangeDate))
            {
                await Send(ctx, Embeds.Error("Invalid gift exchange date"), true);
                return;
            }

            if (participationDeadlineDate >= giftExchangeDate)
            {
                await Send(ctx, Embeds.Error("Participation deadline cannot be after the gift exchange date"), true);
                return;
            }

            santaConfig = new SantaConfig(ctx.Guild!)
            {
                SantaRole = await ctx.Guild!.CreateRoleAsync("Secret Santa", null, DiscordColor.DarkRed),
                ParticipationDeadline = participationDeadlineDate,
                ExchangeDate = giftExchangeDate,
                ExchangeLocation = exchangeLocation,
                PriceLimit = priceLimit
            };
            
            await santaRepo.SaveSantaConfigAsync(santaConfig);

            await Send(ctx, CustomMessages.SantaEventDetails(serverConfig.TimeZone, santaConfig, true));
        }, "santa start");
    }

    [Command("view"), Description("View the Secret Santa event details")]
    public async Task ViewEventDetails(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var santaConfig = await santaRepo.GetSantaConfigAsync(ctx.Guild!);
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!);

            if (santaConfig is null)
            {
                await Send(ctx, Embeds.Error("Secret Santa is inactive"), true);
                return;
            }
            
            await Send(ctx, CustomMessages.SantaEventDetails(serverConfig.TimeZone, santaConfig, false), true);
        }, "santa view");
    }

    [Command("couple"), Description("Assign 2 participants as a couple so they don't get each other")]
    public async Task AssignCouple(SlashCommandContext ctx, DiscordMember first, DiscordMember second)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, first) || await IsBot(ctx, second)) return;
            
            var santaConfig = await santaRepo.GetSantaConfigAsync(ctx.Guild!);
            if (santaConfig is null)
            {
                await Send(ctx, Embeds.Error("Secret Santa is inactive"), true);
                return;
            }

            // === Event started check ===
            if (santaConfig.LockedIn)
            {
                await Send(ctx, Embeds.Error($"{first.Mention} is not a participant in the Secret Santa"), true);
                return;
            }

            var participants = await santaRepo.GetSantaParticipantsAsync(ctx.Guild!);
            
            // === Valid participant checks ===
            var firstParticipant = participants.FirstOrDefault(m => m.Member.User == first);
            if (firstParticipant is null)
            {
                await Send(ctx, Embeds.Error($"{first.Mention} is not a participant in the Secret Santa"), true);
                return;
            }
            
            var secondParticipant = participants.FirstOrDefault(m => m.Member.User == second);
            if (secondParticipant is null)
            {
                await Send(ctx, Embeds.Error($"{second.Mention} is not a participant in the Secret Santa"), true);
                return;
            }
            
            // === Existing SO checks ===

            if (firstParticipant.SignificantOther is not null)
            {
                await Send(ctx, Embeds.Error($"{firstParticipant.Member.DisplayMention} already has a SO assigned: {firstParticipant.SignificantOther.Mention}"), true);
                return;
            }

            if (secondParticipant.SignificantOther is not null)
            {
                await Send(ctx, Embeds.Error($"{secondParticipant.Member.DisplayMention} already has a SO assigned: {secondParticipant.SignificantOther.Mention}"), true);
                return;
            }

            await santaRepo.SaveCoupleAsync(firstParticipant, secondParticipant);
            
            await Send(ctx, Embeds.Success($"Couple successfully assigned: {firstParticipant.Member.DisplayMention} and {secondParticipant.Member.DisplayMention}"));

        }, "santa couple");
    }

    private bool TryGetWinterDate(TimeZoneInfo timeZone, WinterMonths winterMonth, int day, out DateOnly date)
    {
        var todayLocal = TimeZoneHelper.GetCurrentDate(timeZone);
        
        var month = winterMonth switch
        {
            WinterMonths.January => 1,
            WinterMonths.November => 11,
            WinterMonths.December => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(winterMonth), winterMonth, null)
        };

        var year = winterMonth switch
        {
            WinterMonths.January => todayLocal.Year + 1,
            _ => todayLocal.Year
        };

        return DateOnly.TryParse($"{year}-{month}-{day}", out date);
    }
}