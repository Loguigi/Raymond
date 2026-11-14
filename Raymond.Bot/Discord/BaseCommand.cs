using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Raymond.Bot.Discord;

public abstract class BaseCommand(DiscordClient client, ILogger<BaseCommand> logger) : BaseMessageSender(client)
{
    protected async Task ExecuteAsync(SlashCommandContext ctx, Func<Task> action, string commandName)
    {
        try
        {
            logger.LogInformation("{Command} command invoked by {User} in guild {Guild}", 
                commandName, ctx.User.Username, ctx.Guild?.Name);
            
            await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Command} command for user {User}", 
                commandName, ctx.User.Username);

#if DEBUG
            await Send(ctx, Embeds.ErrorDetails(ex, ctx.User, ctx.Guild!), true);
#else
            await Send(ctx, Embeds.Error("An error occurred while executing this command"), true);
            await SendErrorReport(ex, ctx.User, ctx.Guild!);
#endif
        }
    }

    protected async Task<bool> IsBot(SlashCommandContext ctx, DiscordUser user)
    {
        if (user.IsBot)
        {
            await Send(ctx, Embeds.Error("🤖 beep boop"), true);
        }   
        
        return user.IsBot;
    }
}