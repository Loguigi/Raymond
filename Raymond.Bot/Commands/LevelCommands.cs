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
using Raymond.Data.Repositories;
using Raymond.Bot.Utilities;
using Raymond.Shared.Entities;
using Raymond.Shared.Services;

namespace Raymond.Bot.Commands;

public class LevelCommands(
    LevelService levelService,
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
            ServerMember member;
            var members = await memberRepo.GetMembersAsync(ctx.Guild!.Id.ToDbId());
            
            if (user is not null && await IsBot(ctx, user))
            {
                return;
            }
            else if (user is not null)
            {
                member = members.FirstOrDefault(x => x.UserId == user.Id.ToDbId()) ?? throw new Exception("User not found");
            }
            else
            {
                member = members.FirstOrDefault(x => x.UserId == ctx.User.Id.ToDbId()) ?? throw new Exception("User not found");
            }

            var rank = levelService.GetLevelRankings(members).First(m => m.Value.Id == member.Id).Key;
            var prefix = GetRankEmoji(rank);
            user = await member.GetDiscordUser(client);
            var e = new E
            {
                Author = ctx.Guild!.Name,
                AuthorImg = ctx.Guild.IconUrl,
                Title = $"{prefix} {member.DisplayName(user.Username)}",
                Thumbnail = user.AvatarUrl,
                Color = DiscordColor.Orange,
                Fields =
                [
                    new EField("Rank", $"#{rank}", true),
                    new EField("Level", member.Level.ToString(), true),
                    new EField("Experience", $"{member.Experience} / {levelService.ExpNeededForNextLevel(member.Level)} XP")
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
            var rankings = levelService.GetLevelRankings(members);

            await Send(ctx, LevelMessages.Leaderboard(ctx.Guild!, rankings, new LeaderboardId("1")));
        }, "leaderboard");
    }
    
    private static string GetRankEmoji(int rank) => rank switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => ""
    };
}