using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;

namespace Raymond.Bot.Commands;

[Command("config"), Description("Bot config for the server"), RequireGuild]
public class ServerConfigCommands(
    DiscordClient client,
    ILogger<ServerConfigCommands> logger,
    IServerConfigRepository configRepo,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    public enum ToggleType
    {
        [ChoiceDisplayName("Birthday Notifications")]
        BirthdayNotifications,
        [ChoiceDisplayName("Enable Levels")]
        EnableLevels
    }

    public enum ChannelType
    {
        [ChoiceDisplayName("Default Message Channel")]
        DefaultChannel,
        [ChoiceDisplayName("Audit Logging Channel")]
        LoggingChannel
    }

    public enum MessageType
    {
        [ChoiceDisplayName("Welcome - \"(message) (user)\"")]
        WelcomeMessage,
        [ChoiceDisplayName("Leave - \"(user) (message)\"")]
        LeaveMessage,
        [ChoiceDisplayName("Birthday - \"(message) (user)\"")]
        BirthdayMessage,
        [ChoiceDisplayName("Level Up - \"(user) (message)\"")]
        LevelUpMessage
    }

    [Command("toggle"), Description("Server configuration for on/off toggles")]
    public async Task Toggle(SlashCommandContext ctx,
        ToggleType type,
        bool toggle)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var config = await configRepo.GetConfigAsync(ctx.Guild!);
            
            switch (type)
            {
                case ToggleType.BirthdayNotifications:
                    if (toggle == config.BirthdayNotifications)
                    {
                        await Send(ctx, Embeds.Error($"`{type}` is already set to `{toggle}`"));
                        return;
                    }
                    
                    config.BirthdayNotifications = toggle;
                    break;
                case ToggleType.EnableLevels:
                    if (toggle == config.EnableLevels)
                    {
                        await Send(ctx, Embeds.Error($"`{type}` is already set to `{toggle}`"));
                        return;
                    }
                    
                    config.EnableLevels = toggle;
                    break;
            }
            
            await configRepo.SaveConfigAsync(config);
            await Send(ctx, Embeds.ConfigToggle(ctx.Guild!, type.ToString(), toggle));
        }, "config toggle");
    }

    [Command("channel"),
     Description(
         "Set config for the default message channel or the logging channel. Leave blank to reset to default")]
    public async Task Channel(SlashCommandContext ctx,
        ChannelType type,
        DiscordChannel? channel = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var config = await configRepo.GetConfigAsync(ctx.Guild!);
            
            switch (type)
            {
                case ChannelType.DefaultChannel:
                    if (type is ChannelType.DefaultChannel && channel is null)
                    {
                        channel = ctx.Guild!.GetDefaultChannel();
                    }
                    
                    config.DefaultChannel = channel ?? throw new Exception("Guild default channel not found");
                    break;
                case ChannelType.LoggingChannel:
                    config.LoggingChannel = channel;
                    break;
            }
            
            await configRepo.SaveConfigAsync(config);
            await Send(ctx, Embeds.ConfigChannel(ctx.Guild!, type.ToString(), channel));
        }, "config channel");
    }

    [Command("message"), Description("Sets config for messages. Leave blank to reset to default")]
    public async Task Message(SlashCommandContext ctx,
        MessageType type,
        [MinMaxLength(1, 200)] string? message = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var config = await configRepo.GetConfigAsync(ctx.Guild!);
        
            var defaultMessage = type switch
            {
                MessageType.WelcomeMessage or MessageType.LeaveMessage => null,
                MessageType.BirthdayMessage => "Happy Birthday!",
                MessageType.LevelUpMessage => "has leveled up!",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        
            message ??= defaultMessage;
            string? old;

            switch (type)
            {
                case MessageType.WelcomeMessage:
                    old = config.WelcomeMessage;
                    config.WelcomeMessage = message;
                    break;
                case MessageType.LeaveMessage:
                    old = config.LeaveMessage;
                    config.LeaveMessage = message;
                    break;
                case MessageType.BirthdayMessage:
                    old = config.BirthdayMessage;
                    config.BirthdayMessage = message!;
                    break;
                case MessageType.LevelUpMessage:
                    old = config.LevelUpMessage;
                    config.LevelUpMessage = message!;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        
            await configRepo.SaveConfigAsync(config);
            await Send(ctx, Embeds.ConfigMessage(ctx.Guild!, type.ToString(), old, message));
        }, "config message");
    }

    [Command("timezone"), Description("Set the server's time zone ID. Leave blank to reset to UTC")]
    public async Task Timezone(SlashCommandContext ctx,
        [MinMaxLength(1, 100)] string? timezone = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            timezone ??= "UTC";
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out var timeZone))
            {
                await Send(ctx, Embeds.Error("Invalid time zone ID. Use a system time zone ID such as `America/New_York` or `UTC`."));
                return;
            }

            var config = await configRepo.GetConfigAsync(ctx.Guild!);
            var old = config.TimeZoneId;
            config.TimeZoneId = timeZone!.Id;

            await configRepo.SaveConfigAsync(config);
            await Send(ctx, Embeds.ConfigMessage(ctx.Guild!, "TimeZone", old, config.TimeZoneId));
        }, "config timezone");
    }

    [Command("birthdayhour"), Description("Set the birthday notification hour in the server's time zone (0-23)")]
    public async Task BirthdayHour(SlashCommandContext ctx, int hour)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (hour is < 0 or > 23)
            {
                await Send(ctx, Embeds.Error("Birthday notification hour must be between 0 and 23."));
                return;
            }

            var config = await configRepo.GetConfigAsync(ctx.Guild!);
            var old = config.BirthdayTimerHour;
            config.BirthdayTimerHour = hour;

            await configRepo.SaveConfigAsync(config);
            await Send(ctx, Embeds.ConfigMessage(ctx.Guild!, "BirthdayTimerHour", old.ToString(), hour.ToString()));
        }, "config birthdayhour");
    }
}

