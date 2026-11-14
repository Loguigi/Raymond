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

namespace Raymond.Bot.Commands;

[Command("birthday")]
public class BirthdayCommands(
    DiscordClient client,
    IServerMemberRepository memberRepo,
    ILogger<BirthdayCommands> logger,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    [Command("check"), Description("Check a member's birthday"), RequireGuild]
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
                Title = "Birthday",
                Desc = $"# {member.FormattedBirthday}",
                Color = DiscordColor.HotPink
            };
            
            await Send(ctx, e, true);
        }, "birthday check");
    }

    [Command("next"), Description("Check who's birthday is next"), RequireGuild]
    public async Task NextBirthday(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);
            var nextBirthdayMember = memberRepo.GetNextBirthday(members);

            if (nextBirthdayMember == null)
            {
                await Send(ctx, Embeds.Error("No birthday found."), true);
                return;
            }
            
            var daysText = nextBirthdayMember.DaysUntilBirthday switch
            {
                0 => "🎉 Today!",
                1 => "Tomorrow",
                _ => $"In {nextBirthdayMember.DaysUntilBirthday} days"
            };

            var e = new E
            {
                Author = ctx.Guild!.Name,
                AuthorImg = ctx.Guild.IconUrl,
                Title = "Next Birthday",
                Desc =
                    $"# {nextBirthdayMember.DisplayMention}\n\n{nextBirthdayMember.FormattedNextBirthday}\n**{daysText}**",
                Thumbnail = nextBirthdayMember.User.AvatarUrl,
                Color = DiscordColor.HotPink
            };
            
            await Send(ctx, e, true);
        }, "birthday next");
    }

    [Command("list"), Description("Gets a list of all members' birthdays in the server"), RequireGuild]
    public async Task BirthdayList(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);

            await Send(ctx, CustomMessages.BirthdayList(ctx.Guild!, members, new BirthdayListId("1")));
        }, "birthday list");
    }

    [Command("change"), Description("Change a member's birthday"), RequireGuild]
    public async Task ChangeBirthday(SlashCommandContext ctx, DiscordUser user, long year, long month, long day)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            if (!DateTime.TryParse($"{year}-{month}-{day}", out var date)
                || date >= DateTime.Today)
            {
                await Send(ctx, Embeds.Error("Invalid date!"), true);
            }

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, user);

            member.Birthday = date;
            await memberRepo.SaveMemberAsync(member);

            await Send(ctx,
                Embeds.Success($"Birthday for {member.DisplayMention} changed to {member.FormattedBirthday}!"));
        }, "birthday change");
    }

    [Command("add"), Description("Add your birthday date"), RequireGuild]
    public async Task AddBirthday(SlashCommandContext ctx, long year, long month, long day)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!DateTime.TryParse($"{year}-{month}-{day}", out var date)
                || date >= DateTime.Today)
            {
                await Send(ctx, Embeds.Error("Invalid date!"), true);
            }

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, ctx.User);

            member.Birthday = date;
            await memberRepo.SaveMemberAsync(member);

            await Send(ctx,
                Embeds.Success($"Your birthday is set to {member.FormattedBirthday}!"));
        }, "birthday add");
    }
}