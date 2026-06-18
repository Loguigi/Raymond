using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Repositories;
using Singularity.Raymond.Utilities;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Commands;

[Command("birthday")]
public class BirthdayCommands(
    ServerMemberRepository memberRepo,
    ILogger<BirthdayCommands> logger)
    : BaseCommand(logger)
{
    [Command("check"), Description("Check a member's birthday"), RequireGuild]
    public async Task CheckBirthday(SlashCommandContext ctx, DiscordUser user)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, user);
            if (member?.Birthday == null)
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
    public async Task NextBirthday(SlashCommandContext ctx, DiscordUser user)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;
            
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);
            var nextBirthdayMember = memberRepo.GetNextBirthday(members);
            
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
}