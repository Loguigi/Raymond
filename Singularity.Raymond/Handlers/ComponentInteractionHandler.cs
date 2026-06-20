using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Models;
using Singularity.Raymond.Repositories;
using Singularity.Raymond.Utilities;
using Singularity.Shared.Discord;

namespace Singularity.Raymond.Handlers;

public class ComponentInteractionHandler(
    DiscordClient client,
    ServerMemberRepository memberRepo,
    WheelPickerRepository wheelRepo,
    Random random,
    ILogger<ComponentInteractionHandler> logger)
    : BaseHandler(client, logger), IEventHandler<ComponentInteractionCreatedEventArgs>
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
            IdPrefixes.Identities => HandleIdentities(e),
            IdPrefixes.CoinFlip => HandleCoinFlip(e),
            _ => Task.CompletedTask
        };
    }

    private async Task HandleLeaderboard(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var members = await memberRepo.GetMembersAsync(e.Guild);
            if (!LeaderboardId.TryParse(e.Id, out var id) || id is null) return;

            await Update(e, CustomMessages.Leaderboard(e.Guild, members, id));
        }, "leaderboard", e.Id);
    }

    private async Task HandleBirthdayList(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var members = await memberRepo.GetMembersAsync(e.Guild);
            if (!BirthdayListId.TryParse(e.Id, out var id) || id is null) return;

            await Update(e, CustomMessages.BirthdayList(e.Guild, members, id));
        },  "birthday_list", e.Id);
    }

    private async Task HandleIdentities(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var members = await memberRepo.GetMembersAsync(e.Guild);
            if (!IdentitiesId.TryParse(e.Id, out var id) || id is null) return;
            
            await Update(e, CustomMessages.Identities(e.Guild!, members, id));
        }, "identities", e.Id);
    }
    
    private async Task HandleCoinFlip(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            var flip = random.Next(2) == 1 ? FlipResult.Heads : FlipResult.Tails;
            var member = await memberRepo.GetMemberAsync(e.Guild, e.User);
            if (!CoinFlipId.TryParse(e.Id, out var id) || id is null) return;
            
            var newId = new CoinFlipId(flip, flip == FlipResult.Heads ? id.HeadCount + 1 : id.HeadCount, flip == FlipResult.Tails ? id.TailsCount + 1 : id.TailsCount);
            
            await Update(e, CustomMessages.CoinFlip(member, newId));
        }, "coin_flip", e.Id);
    }
}