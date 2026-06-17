using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Models;
using Singularity.Raymond.Repositories;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Commands;

public class LevelCommands(
    ILogger<BaseCommand> logger,
    ServerMemberRepository memberRepo)
    : BaseCommand(logger)
{
    [Command("level"), RequireGuild, Description("Displays your level card or another user's level card")]
    public async Task Level(SlashCommandContext ctx, DiscordUser? user = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            ServerMember member;
            var members = await memberRepo.GetMembersAsync(ctx.Guild!);
            
            if (user is not null && await IsBot(ctx, user))
            {
                return;
            }
            else if (user is not null)
            {
                member = members.FirstOrDefault(x => x.User == user) ?? throw new Exception("User not found");
            }
            else
            {
                member = members.FirstOrDefault(x => x.User == ctx.User) ?? throw new Exception("User not found");
            }
            
            var prefix = GetRankEmoji(member.Rank);
            var e = new E
            {
                Author = ctx.Guild!.Name,
                AuthorImg = ctx.Guild.IconUrl,
                Title = $"{prefix} {member.DisplayName}",
                Color = DiscordColor.Orange,
                Fields =
                [
                    new EField("Rank", $"#{member.Rank}", true),
                    new EField("Level", member.Level.ToString(), true),
                    new EField("Experience", $"{member.Experience} / {member.ExpNeededForNextLevel} XP")
                ]
            };
            
            await Send(ctx, e);
        }, "level");
    }
    
    [Command("leaderboard")]
    
    private static string GetRankEmoji(int rank) => rank switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => ""
    };
}