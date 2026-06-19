using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Repositories;
using Singularity.Raymond.Utilities;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Handlers;

public class ComponentInteractionHandler(
    ServerMemberRepository memberRepo,
    WheelPickerRepository wheelRepo,
    Random random,
    ILogger<ComponentInteractionHandler> logger)
    : BaseHandler(logger), IEventHandler<ComponentInteractionCreatedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient s, ComponentInteractionCreatedEventArgs e)
    {
        var id = e.Id.Split(':').First();
        logger.LogDebug("Component interaction received: {ComponentId} from {User} in {Guild}", 
            id, e.User.Username, e.Guild.Name);

        await RouteInteraction(id, e);
    }

    private Task RouteInteraction(string id, ComponentInteractionCreatedEventArgs e)
    {
        return id switch
        {
            IdPrefixes.Leaderboard => HandleLeaderboard(e),
            IdPrefixes.BirthdayList => HandleBirthdayList(e),
            _ => Task.CompletedTask
        };
    }

    private async Task HandleLeaderboard(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var members = await memberRepo.GetMembersAsync(e.Guild);
            if (!LeaderboardId.TryParse(e.Id, out var id) || id is null) return;

            await Send(e, CustomMessages.Leaderboard(e.Guild, members, id));
        }, "leaderboard", e.Id);
    }

    private async Task HandleBirthdayList(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var members = await memberRepo.GetMembersAsync(e.Guild);
            if (!BirthdayListId.TryParse(e.Id, out var id) || id is null) return;

            await Send(e, CustomMessages.BirthdayList(e.Guild, members, id));
        },  "birthday_list", e.Id);
    }
}