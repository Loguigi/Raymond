using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Repositories;
using Singularity.Raymond.Utilities;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Commands;

public class MiscCommands(
    ServerMemberRepository memberRepo,
    Random random,
    ILogger<MiscCommands> logger)
    : BaseCommand(logger)
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
}