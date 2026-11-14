using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Bot.Utilities;
using Raymond.Data.Models;
using Raymond.Data.Repositories;

namespace Raymond.Bot.Commands;

public class MiscCommands(
    DiscordClient client,
    IServerMemberRepository memberRepo,
    Random random,
    ILogger<MiscCommands> logger)
    : BaseCommand(client, logger)
{
    [Command("who"), Description("Displays a list of all members' names in the server"), RequireGuild]
    public async Task Who(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);

            await Send(ctx, CustomMessages.Identities(ctx.Guild!, members, new IdentitiesId("1")));
        }, "who");
    }

    [Command("whois"), Description("Displays a member's name if set")]
    public async Task WhoIs(SlashCommandContext ctx, DiscordUser user)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (await IsBot(ctx, user)) return;

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, user);
            if (member.Name == null)
            {
                await Send(ctx, Embeds.Error("This user does not have a name set"), true);
                return;
            }

            var e = new E
            {
                Author = member.User.Username,
                AuthorImg = member.User.AvatarUrl,
                Title = "Who are you?",
                Desc = $"# **{member.Name}**",
                Color = DiscordColor.Aquamarine
            };
            
            await Send(ctx, e, true);
        }, "whois");
    }

    [Command("iam"), Description("Set your name")]
    public async Task SetName(SlashCommandContext ctx, [MaxLength(50)] string name)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var member = await memberRepo.GetMemberAsync(ctx.Guild!, ctx.User);
            member.Name = name;
            await memberRepo.SaveMemberAsync(member);
            
            await Send(ctx, Embeds.Success($"Your name is now **{name}!**"), true);
        }, "set-name");
    }
    
    [Command("flip"), RequireGuild, Description("Flip a coin! Heads or tails?"),]
    public async Task Flip(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var flip = random.Next(2) == 1 ? FlipResult.Heads : FlipResult.Tails;
            var member = await memberRepo.GetMemberAsync(ctx.Guild!, ctx.User);

            await Send(ctx, CustomMessages.CoinFlip(member, new CoinFlipId(flip, flip == FlipResult.Heads ? 1 : 0, flip == FlipResult.Tails ? 1 : 0)));
        }, "flip");
    }
}