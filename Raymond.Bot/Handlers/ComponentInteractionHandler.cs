using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Utilities;
using Raymond.Data.Models;
using Raymond.Data.Repositories;
using BaseHandler = Raymond.Bot.Discord.BaseHandler;

namespace Raymond.Bot.Handlers;

public class ComponentInteractionHandler(
    DiscordClient client,
    IServerMemberRepository memberRepo,
    IWheelPickerRepository wheelRepo,
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
            IdPrefixes.WheelSpin => HandleWheelSpin(e),
            IdPrefixes.WheelList => HandleWheelList(e),
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
            if (!BirthdayListId.TryParse(e.Id, out var id) || id is null) return;
            var members = await memberRepo.GetMembersAsync(e.Guild);

            await Update(e, CustomMessages.BirthdayList(e.Guild, members, id));
        },  "birthday_list", e.Id);
    }

    private async Task HandleIdentities(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            if (!IdentitiesId.TryParse(e.Id, out var id) || id is null) return;
            var members = await memberRepo.GetMembersAsync(e.Guild);
            
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
    
    private async Task HandleWheelSpin(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            if (!WheelSpinId.TryParse(e.Id, out var id) || id is null) return;
            var member = await memberRepo.GetMemberAsync(e.Guild, e.User);
            var wheel = (await wheelRepo.GetAllAsync(e.Guild)).First(w => w.Id == id.WheelId);
            var option = wheel.Options.First(o => o.Option == id.OptionSpun);
            
            if (id.RemoveOption)
            {
                option = option with { TempRemoved = true };
                var optionIndex = wheel.Options.FindIndex(o => o.Option == id.OptionSpun);
                wheel.Options[optionIndex] = option;
                await wheelRepo.SaveWheelOptionAsync(wheel, option);
            }

            if (wheel.AvailableOptions.Count == 0) return;
            
            var result = wheel.AvailableOptions[random.Next(wheel.AvailableOptions.Count)];
            id = new WheelSpinId(wheel.Id, id.SpinCount + 1, result.Option, id.OptionSpun, id.RemoveOption);
            
            await Update(e, CustomMessages.WheelSpin(member, wheel, id));
        }, "wheel_spin", e.Id);
    }

    private async Task HandleWheelList(ComponentInteractionCreatedEventArgs e)
    {
        await ExecuteAsync(e, async () =>
        {
            if (!WheelOptionListId.TryParse(e.Id, out var id) || id is null) return;
            var wheel = (await wheelRepo.GetAllAsync(e.Guild)).First(w => w.Id == id.WheelId);
            
            await Update(e, CustomMessages.WheelOptionList(wheel, id));
        }, "wheel_option_list", e.Id);
    }
}