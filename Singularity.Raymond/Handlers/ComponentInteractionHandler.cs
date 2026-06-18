using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Singularity.Raymond.Repositories;
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
        
        
    }
}