using System.ComponentModel;
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
using Raymond.Bot.Utilities;
using Raymond.Data.Repositories;
using Raymond.Shared.Domain;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Commands;

public class LevelCommands(
    LevelCalculator levelCalculator,
    ServerMemberService memberService,
    DiscordClient client,
    ILogger<BaseCommand> logger,
    IServerMemberRepository memberRepo,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    [Command("level"), RequireGuild, Description("Displays your level card or another user's level card")]
    public async Task Level(SlashCommandContext ctx, DiscordUser? user = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (user is not null && await IsBot(ctx, user))
            {
                return;
            }

            var members = await memberRepo.GetMembersAsync(ctx.Guild!.Id.ToDbId());
            var member = await memberService.GetMemberByUserAsync(ctx.Guild!, user ?? ctx.User);

            var rank = levelCalculator.GetLevelRankings(members).FirstOrDefault(m => m.Value.Id == member.Id).Key;
            if (rank == 0)
            {
                await Send(ctx, Embeds.Error("User not found"), true);
                return;
            }

            var prefix = LevelMessages.GetRankEmoji(rank);
            var discordUser = await member.GetDiscordUser(client);
            var e = new E
            {
                Author = ctx.Guild!.Name,
                AuthorImg = ctx.Guild.IconUrl,
                Title = $"{prefix} {member.DisplayName(discordUser.Username)}",
                Thumbnail = discordUser.AvatarUrl,
                Color = DiscordColor.Orange,
                Fields =
                [
                    new EField("Rank", $"#{rank}", true),
                    new EField("Level", member.Level.ToString(), true),
                    new EField("Experience", $"{member.Experience} / {levelCalculator.ExpNeededForNextLevel(member.Level)} XP")
                ]
            };

            await Send(ctx, e);
        }, "level");
    }

    [Command("leaderboard")]
    public async Task Leaderboard(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var members = await memberRepo.GetMembersAsync(ctx.Guild!.Id.ToDbId());
            var rankings = levelCalculator.GetLevelRankings(members);

            await Send(ctx, LevelMessages.Leaderboard(ctx.Guild!, rankings, new LeaderboardId("1")));
        }, "leaderboard");
    }
}
