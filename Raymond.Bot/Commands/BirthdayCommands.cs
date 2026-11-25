using System.ComponentModel;
using System.Globalization;
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
using Raymond.Data.Repositories;
using Raymond.Bot.Utilities;
using Raymond.Shared.Domain;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Commands;

[Command("birthday"), RequireGuild]
public class BirthdayCommands(
    DiscordClient client,
    IServerMemberRepository memberRepo,
    BirthdayRules birthdayRules,
    ServerMemberService memberService,
    IServerConfigRepository configRepo,
    ILogger<BirthdayCommands> logger,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    private readonly DiscordClient _client = client;

    [Command("check"), Description("Check a member's birthday")]
    public async Task CheckBirthday(SlashCommandContext ctx, DiscordUser user)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            var member = await memberService.GetMemberByUserAsync(ctx.Guild!, user);
            if (!member.Birthday.HasValue)
            {
                await Send(ctx, Embeds.Error("No birthday found."), true);
                return;
            }

            var e = new E
            {
                Author = member.DisplayName(user.Username),
                AuthorImg = user.AvatarUrl,
                Title = "🎂 Birthday",
                Desc = $"# {member.Birthday.ToDiscordFormat()}",
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
            var members = await memberRepo.GetMembersAsync(ctx.Guild!.Id.ToDbId());
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            
            var localDate = TimeZoneHelper.GetCurrentDate(serverConfig.TimeZone);
            var nextBirthdayMember = birthdayRules.GetNextBirthday(members, localDate);

            if (nextBirthdayMember == null)
            {
                await Send(ctx, Embeds.Error("No birthday found."), true);
                return;
            }
            
            var daysUntilBirthday = birthdayRules.GetDaysUntilBirthday(nextBirthdayMember, localDate);
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
                    $"# {nextBirthdayMember.DisplayMention()}\n\n{birthdayRules.GetNextBirthday(nextBirthdayMember, localDate).ToDiscordFormat()}\n**{daysText}**",
                Thumbnail = (await nextBirthdayMember.GetDiscordUser(_client)).AvatarUrl,
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
            var members = await memberRepo.GetMembersAsync(ctx.Guild!.Id.ToDbId());
            var serverConfig = await configRepo.GetConfigAsync(ctx.Guild!.Id.ToDbId());
            ArgumentNullException.ThrowIfNull(serverConfig);
            
            var localDate = TimeZoneHelper.GetCurrentDate(serverConfig.TimeZone);

            await Send(ctx, BirthdayMessages.BirthdayList(ctx.Guild!, birthdayRules, members, localDate, new BirthdayListId("1")));
        }, "birthday list");
    }

    [Command("change"), Description("Change a member's birthday")]
    public async Task ChangeBirthday(SlashCommandContext ctx, DiscordUser user, long year, long month, long day)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            if (!birthdayRules.TryParseBirthday(year, month, day, out var date))
            {
                await Send(ctx, Embeds.Error("Invalid date!"), true);
                return;
            }

            var member = await memberService.SetBirthdayAsync(ctx.Guild!, user, date);

            await Send(ctx,
                Embeds.Success($"Birthday for {member.DisplayMention()} changed to {date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)}!"));
        }, "birthday change");
    }

    [Command("add"), Description("Add your birthday date")]
    public async Task AddBirthday(SlashCommandContext ctx, long year, long month, long day)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!birthdayRules.TryParseBirthday(year, month, day, out var date))
            {
                await Send(ctx, Embeds.Error("Invalid date!"), true);
                return;
            }

            var member = await memberService.SetBirthdayAsync(ctx.Guild!, ctx.User, date);

            await Send(ctx,
                Embeds.Success($"Your birthday is set to {date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)}!"));
        }, "birthday add");
    }
}