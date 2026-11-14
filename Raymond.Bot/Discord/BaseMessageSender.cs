using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;

namespace Raymond.Bot.Discord;

public abstract class BaseMessageSender(DiscordClient client, IConfiguration config)
{
    protected async Task SendErrorReport(Exception ex, DiscordUser? userExecuted, DiscordGuild? guildExecuted, string? id = null)
    {
        var adminId = ulong.Parse(config["AdminId"] ?? throw new Exception("AdminId not set"));
        var adminUser = await client.GetUserAsync(adminId);
        
        await SendDm(adminUser, Embeds.ErrorDetails(ex, userExecuted, guildExecuted, id));
    }
    
    #region Slash Command Senders
    protected async Task Send(SlashCommandContext ctx, M message, bool ephemeral = false)
    {
        var builder = new DiscordInteractionResponseBuilder(message.Build())
        {
            IsEphemeral = ephemeral
        };
        
        await SendAsync(ctx.Interaction, builder, DiscordInteractionResponseType.ChannelMessageWithSource);
    }

    protected async Task Send(SlashCommandContext ctx, E embed, bool ephemeral = false)
    {
        var builder = new DiscordInteractionResponseBuilder(new DiscordMessageBuilder().AddEmbed(embed.Build()))
        {
            IsEphemeral = ephemeral
        };
        
        await SendAsync(ctx.Interaction, builder, DiscordInteractionResponseType.ChannelMessageWithSource);
    }

    protected async Task FollowUp(SlashCommandContext ctx, M message, bool ephemeral = false)
    {
        await FollowUpAsync(ctx.Interaction, new DiscordFollowupMessageBuilder(message.Build()).AsEphemeral());
    }

    protected async Task FollowUp(SlashCommandContext ctx, E embed, bool ephemeral = false)
    {
        await FollowUpAsync(ctx.Interaction, new DiscordFollowupMessageBuilder(new DiscordMessageBuilder().AddEmbed(embed.Build())).AsEphemeral());
    }
    #endregion

    #region Event Handler Senders
    protected async Task Send(ComponentInteractionCreatedEventArgs ctx, M message, bool ephemeral = false)
    {
        var builder = new DiscordInteractionResponseBuilder(message.Build())
        {
            IsEphemeral = ephemeral
        };
        
        await SendAsync(ctx.Interaction, builder, DiscordInteractionResponseType.ChannelMessageWithSource);
    }

    protected async Task Send(DiscordChannel channel, M message)
    {
        await SendAsync(channel, message.Build());
    }

    protected async Task Send(DiscordChannel channel, E embed)
    {
        await SendAsync(channel, new DiscordMessageBuilder().AddEmbed(embed.Build()));
    }

    protected async Task Update(ComponentInteractionCreatedEventArgs ctx, M message)
    {
        var builder = new DiscordInteractionResponseBuilder(message.Build());
        
        await SendAsync(ctx.Interaction, builder, DiscordInteractionResponseType.UpdateMessage);
    }
    #endregion
    
    #region DM Senders

    protected async Task SendDm(DiscordUser user, M message)
    {
        var builder = new DiscordMessageBuilder(message.Build());
        var dmChannel = await user.CreateDmChannelAsync();
        
        await SendAsync(dmChannel, builder);
    }
    
    protected async Task SendDm(DiscordUser user, E embed)
    {
        var builder = new DiscordMessageBuilder().AddEmbed(embed.Build());
        var dmChannel = await user.CreateDmChannelAsync();
        
        await SendAsync(dmChannel, builder);
    }
    #endregion
    
    #region Private Helpers
    private static async Task SendAsync(DiscordInteraction interaction, DiscordInteractionResponseBuilder builder, DiscordInteractionResponseType type)
    {
        await interaction.CreateResponseAsync(type, builder);
    }

    private static async Task SendAsync(DiscordChannel channel, DiscordMessageBuilder message)
    {
        await channel.SendMessageAsync(message);
    }

    private async Task SendAsync(DiscordDmChannel dmChannel, DiscordMessageBuilder message)
    {
        await dmChannel.SendMessageAsync(message);
    }

    private async Task FollowUpAsync(DiscordInteraction interaction, DiscordFollowupMessageBuilder builder)
    {
        await interaction.CreateFollowupMessageAsync(builder);
    }
    #endregion
}