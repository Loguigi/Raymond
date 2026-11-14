using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Raymond.Bot.Discord;

public abstract class BaseHandler(DiscordClient client, ILogger<BaseHandler> logger) : BaseMessageSender(client)
{
    protected async Task ExecuteAsync(ComponentInteractionCreatedEventArgs ctx, Func<Task> action, string handler, string id)
    {
        try
        {
            logger.LogInformation("{Handler} handler invoked by {User} in guild {Guild} with ID {ID}", 
                handler, ctx.User.Username, ctx.Guild.Name, id);
            
            await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Handler} handler for user {User} with ID {ID}", 
                handler, ctx.User.Username, id);
            
#if DEBUG
            await Send(ctx, new M { Embed = Embeds.ErrorDetails(ex, ctx.User, ctx.Guild!) });
#else
            await Send(ctx, new M { Embed = Embeds.Error("An error occurred while executing this command") });
            await SendErrorReport(ex, ctx.User, ctx.Guild!, id);
#endif
        }
    }
    
    protected async Task ExecuteAsync(MessageCreatedEventArgs ctx, Func<Task> action, string handler)
    {
        await ExecuteHandler(ctx.Author, ctx.Guild, action, handler);
    }
    
    protected async Task ExecuteAsync(ScheduledGuildEventCreatedEventArgs ctx, Func<Task> action, string handler, string id)
    {
        await ExecuteHandlerWithId(ctx.Creator, ctx.Guild, action, handler, id);
    }

    protected async Task ExecuteAsync(GuildMemberAddedEventArgs ctx, Func<Task> action, string handler)
    {
        await ExecuteHandler(ctx.Member, ctx.Guild, action, handler);
    }
    
    protected async Task ExecuteAsync(GuildMemberRemovedEventArgs ctx, Func<Task> action, string handler)
    {
        await ExecuteHandler(ctx.Member, ctx.Guild, action, handler);
    }

    private async Task ExecuteHandler(DiscordUser user, DiscordGuild guild, Func<Task> action, string handler)
    {
        try
        {
            logger.LogInformation("{Handler} handler invoked by {User} in guild {Guild}", handler, user.Username, guild.Name);

            await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Handler} handler for user {User}", 
                handler, user.Username);
            
            await SendErrorReport(ex, user, guild);
        }
    }

    private async Task ExecuteHandlerWithId(DiscordUser user, DiscordGuild guild, Func<Task> action, string handler,
        string id)
    {
        try
        {
            logger.LogInformation("{Handler} handler invoked by {User} in guild {Guild} with ID {ID}", 
                handler, user.Username, guild.Name, id);
            
            await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Handler} handler for user {User} with ID {ID}", 
                handler, user.Username, id);
            
            await SendErrorReport(ex, user, guild, id);
        }
    }
}