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
using Raymond.Bot.Services;

namespace Raymond.Bot.Commands;

[Command("config"), Description("Bot config for the server"), RequireGuild]
public class ServerConfigCommands(
    DiscordClient client,
    ILogger<ServerConfigCommands> logger,
    ServerConfigService configService,
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
            var result = await configService.UpdateConfigAsync(ctx.Guild!, config =>
            {
                switch (type)
                {
                    case ToggleType.BirthdayNotifications:
                        if (toggle == config.BirthdayNotifications)
                        {
                            return $"`{type}` is already set to `{toggle}`";
                        }

                        config.BirthdayNotifications = toggle;
                        break;
                    case ToggleType.EnableLevels:
                        if (toggle == config.EnableLevels)
                        {
                            return $"`{type}` is already set to `{toggle}`";
                        }

                        config.EnableLevels = toggle;
                        break;
                }

                return null;
            });

            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

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
            DiscordChannel? effectiveChannel = channel;

            var result = await configService.UpdateConfigAsync(ctx.Guild!, config =>
            {
                switch (type)
                {
                    case ChannelType.DefaultChannel:
                        effectiveChannel ??= ctx.Guild!.GetDefaultChannel();
                        config.DefaultChannelId = effectiveChannel?.Id.ToDbId() ?? throw new Exception("Guild default channel not found");
                        break;
                    case ChannelType.LoggingChannel:
                        config.LoggingChannelId = channel?.Id.ToDbId();
                        break;
                }

                return null;
            });

            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.ConfigChannel(ctx.Guild!, type.ToString(), effectiveChannel));
        }, "config channel");
    }

    [Command("message"), Description("Sets config for messages. Leave blank to reset to default")]
    public async Task Message(SlashCommandContext ctx,
        MessageType type,
        [MinMaxLength(1, 200)] string? message = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var defaultMessage = type switch
            {
                MessageType.WelcomeMessage or MessageType.LeaveMessage => null,
                MessageType.BirthdayMessage => "Happy Birthday!",
                MessageType.LevelUpMessage => "has leveled up!",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            message ??= defaultMessage;
            string? old = null;

            var result = await configService.UpdateConfigAsync(ctx.Guild!, config =>
            {
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
                }

                return null;
            });

            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

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

            string? old = null;
            var result = await configService.UpdateConfigAsync(ctx.Guild!, config =>
            {
                old = config.TimeZoneId;
                config.TimeZoneId = timeZone!.Id;
                return null;
            });

            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.ConfigMessage(ctx.Guild!, "Time Zone", old, timeZone.Id));
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

            string? old = null;
            var result = await configService.UpdateConfigAsync(ctx.Guild!, config =>
            {
                old = config.BirthdayTimerHour.ToString();
                config.BirthdayTimerHour = hour;
                return null;
            });

            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.ConfigMessage(ctx.Guild!, "Birthday Notification Hour", old, hour.ToString()));
        }, "config birthdayhour");
    }
}
