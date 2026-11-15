using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Bot.Utilities;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Commands;

[Command("birthday"), RequireGuild]
public class BirthdayCommands(
    DiscordClient client,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    ILogger<BirthdayCommands> logger,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    [Command("check"), Description("Check a member's birthday")]
    public async Task CheckBirthday(SlashCommandContext ctx, DiscordUser user)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, user);
            if (!member.Birthday.HasValue)
            {
                await Send(ctx, Embeds.Error("No birthday found."), true);
                return;
            }

            var e = new E
            {
                Author = member.DisplayName,
                AuthorImg = member.User.AvatarUrl,
                Title = "🎂 Birthday",
                Desc = $"# {member.GetFormattedBirthday(member.Birthday)}",
                Color = DiscordColor.HotPink
            };
            
            await Send(ctx, e, true);
        }, "birthday check");
    }

    [Command("next"), Description("Check who's birthday is next")]
    public async Task NextBirthday(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!);
            var localDate = TimeZoneHelper.GetCurrentDate(serverConfig.TimeZone);
            var nextBirthdayMember = memberRepo.GetNextBirthday(members, localDate);

            if (nextBirthdayMember == null)
            {
                await Send(ctx, Embeds.Error("No birthday found."), true);
                return;
            }
            
            var daysUntilBirthday = nextBirthdayMember.GetDaysUntilBirthday(localDate);
            var daysText = daysUntilBirthday switch
            {
                0 => "🎉 Today!",
                1 => "Tomorrow",
                _ => $"In {daysUntilBirthday} days"
            };

            var e = new E
            {
                Author = ctx.Guild!.Name,
                AuthorImg = ctx.Guild.IconUrl,
                Title = "Next Birthday",
                Desc =
                    $"# {nextBirthdayMember.DisplayMention}\n\n{nextBirthdayMember.GetFormattedBirthday(nextBirthdayMember.GetNextBirthday(localDate))}\n**{daysText}**",
                Thumbnail = nextBirthdayMember.User.AvatarUrl,
                Color = DiscordColor.HotPink
            };
            
            await Send(ctx, e, true);
        }, "birthday next");
    }

    [Command("list"), Description("Gets a list of all members' birthdays in the server")]
    public async Task BirthdayList(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!);
            var localDate = TimeZoneHelper.GetCurrentDate(serverConfig.TimeZone);

            await Send(ctx, CustomMessages.BirthdayList(ctx.Guild!, members, localDate, new BirthdayListId("1")));
        }, "birthday list");
    }

    [Command("change"), Description("Change a member's birthday")]
    public async Task ChangeBirthday(SlashCommandContext ctx, DiscordUser user, long year, long month, long day)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            if (!DateOnly.TryParse($"{year}-{month}-{day}", out var date)
                || date >= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                await Send(ctx, Embeds.Error("Invalid date!"), true);
                return;
            }

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, user);

            member.Birthday = date;
            await memberRepo.SaveMemberAsync(member);

            await Send(ctx,
                Embeds.Success($"Birthday for {member.DisplayMention} changed to {member.GetFormattedBirthday(date)}!"));
        }, "birthday change");
    }

    [Command("add"), Description("Add your birthday date")]
    public async Task AddBirthday(SlashCommandContext ctx, long year, long month, long day)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!DateOnly.TryParse($"{year}-{month}-{day}", out var date)
                || date >= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                await Send(ctx, Embeds.Error("Invalid date!"), true);
                return;
            }

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, ctx.User);

            member.Birthday = date;
            await memberRepo.SaveMemberAsync(member);

            await Send(ctx,
                Embeds.Success($"Your birthday is set to {member.GetFormattedBirthday(date)}!"));
        }, "birthday add");
    }
}