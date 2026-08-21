using Artifice.Discord;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Raymond.Data.Repositories;

namespace Raymond.Bot.Commands;

[Command("mc"), RequireGuild]
public class MinecraftCommands(
    MinecraftRepository mcRepo,
    DiscordClient client,
    ILogger<MinecraftCommands> logger)
    : BaseCommand(client, logger) 
{
    [Command("info")]
    public async Task Info(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var info = await mcRepo.GetInfoAsync(ctx.Guild!);
            var players = await mcRepo.GetPlayerListAsync(info);

            List<EField> fields =
            [
                new("Status", await mcRepo.IsServerOnline(info) ? "🟢 Online" : "🔴 Offline", true),
                new("Players", $"{players.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length} / {info.MaxPlayers}", true),
                new("IP", $"{info.Ip}", true),
            ];
            
            if (info.Mods is not null) fields.Add(new EField("Mods", info.Mods));

            var embed = new E
            {
                Author = "Minecraft",
                AuthorImg =
                    "https://cdn.discordapp.com/attachments/1129734051016740886/1534733572664070395/image.png?ex=6a753385&is=6a73e205&hm=19e2c78dc6c2694649dbf8498c1621cb1b6d378f0638da993b7bcd6860e7b00a&",
                Color = DiscordColor.DarkGreen,
                Title = info.Name,
                Thumbnail = info.ImageUrl,
                Desc = players,
                Timestamp = true,
                Fields = fields
            };

            await Send(ctx, embed);
        }, "mc info");
    }
    
    [Command("setup")]
    public async Task Setup(SlashCommandContext ctx,
        string? container = null,
        string? ip = null,
        string? name = null,
        string? image = null,
        int? maxPlayers = null,
        string? mods = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!IsAdminAuthorized(ctx.User)) return;

            var info = await mcRepo.GetInfoAsync(ctx.Guild!);
            info.ContainerName = container ?? info.ContainerName;
            info.Ip = ip ?? info.Ip;
            info.Name = name ?? info.Name;
            info.ImageUrl = image ?? info.ImageUrl;
            info.MaxPlayers = maxPlayers ?? info.MaxPlayers;
            if (string.IsNullOrWhiteSpace(info.Mods))
            {
                info.Mods = mods ?? info.Mods;
            }
            else if (!string.IsNullOrWhiteSpace(mods))
            {
                info.Mods = mods;
            }

            await mcRepo.SaveInfoAsync(info);

            await Send(ctx, Embeds.Success("Saved server info"), true);
        }, "mc command");
    }
    
    [Command("command")]
    public async Task Command(SlashCommandContext ctx, string command)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!IsAdminAuthorized(ctx.User))
            {
                await Send(ctx, Embeds.Error("You are not authorized to use this command."), true);
                return;
            }
            
            var info = await mcRepo.GetInfoAsync(ctx.Guild!);

            if (!await mcRepo.IsServerOnline(info))
            {
                await Send(ctx, Embeds.Error("Server is offline"), true);
                return;
            }
            
            var output = await mcRepo.ExecuteCommandAsync(info, command);

            if (string.IsNullOrWhiteSpace(output))
            {
                await ctx.RespondAsync("Success", true);
                return;
            }
            
            var embed = new E
            {
                Author = "Minecraft",
                AuthorImg =
                    "https://cdn.discordapp.com/attachments/1129734051016740886/1534733572664070395/image.png?ex=6a753385&is=6a73e205&hm=19e2c78dc6c2694649dbf8498c1621cb1b6d378f0638da993b7bcd6860e7b00a&",
                Color = DiscordColor.DarkGreen,
                Title = info.Name,
                Thumbnail = info.ImageUrl,
                Desc = output,
                Timestamp = true,
            };

            await Send(ctx, embed);
        }, "mc command");
    }
    
    [Command("start")]
    public async Task Start(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var info = await mcRepo.GetInfoAsync(ctx.Guild!);
            var succeeded = await mcRepo.StartServerAsync(info);
            
            var embed = new E
            {
                Author = "Minecraft",
                AuthorImg =
                    "https://cdn.discordapp.com/attachments/1129734051016740886/1534733572664070395/image.png?ex=6a753385&is=6a73e205&hm=19e2c78dc6c2694649dbf8498c1621cb1b6d378f0638da993b7bcd6860e7b00a&",
                Color = succeeded ? DiscordColor.DarkGreen : DiscordColor.DarkRed,
                Title = info.Name,
                Thumbnail = info.ImageUrl,
                Desc = succeeded ? "Starting server..." : "Server is already running",
                Timestamp = true,
            };

            await Send(ctx, embed);

            if (!succeeded) return;
            await Task.Delay(TimeSpan.FromSeconds(10));
            var online = await mcRepo.IsServerOnline(info);
            
            embed.Desc = online ? "Server started successfully" : "Failed to start server";
            embed.Color = online ? DiscordColor.DarkGreen : DiscordColor.DarkRed;

            await FollowUp(ctx, embed);
        }, "mc start");
    }
    
    [Command("stop")]
    public async Task Stop(SlashCommandContext ctx)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (!IsAdminAuthorized(ctx.User))
            {
                await Send(ctx, Embeds.Error("You are not authorized to use this command."), true);
                return;
            }
            
            var info = await mcRepo.GetInfoAsync(ctx.Guild!);
            
            if (await mcRepo.IsServerOnline(info))
            {
                await Send(ctx, Embeds.Error("Server is already offline"), true);
                return;
            }
            
            var embed = new E
            {
                Author = "Minecraft",
                AuthorImg =
                    "https://cdn.discordapp.com/attachments/1129734051016740886/1534733572664070395/image.png?ex=6a753385&is=6a73e205&hm=19e2c78dc6c2694649dbf8498c1621cb1b6d378f0638da993b7bcd6860e7b00a&",
                Color = DiscordColor.DarkGreen,
                Title = info.Name,
                Thumbnail = info.ImageUrl,
                Desc = "Shutting down server...",
                Timestamp = true,
            };
            
            await Send(ctx, embed);
            await mcRepo.StopServerAsync(info);
            await Task.Delay(TimeSpan.FromSeconds(15));
            
            var online = await mcRepo.IsServerOnline(info);
            embed.Desc = online ? "Fail: Server is still online" : "Server is offline";
            embed.Color = online ? DiscordColor.DarkRed : DiscordColor.DarkGreen;
            
            await FollowUp(ctx, embed);
        }, "mc stop");
    }

    private bool IsAdminAuthorized(DiscordUser user)
    {
        return user.Id == ulong.Parse(Environment.GetEnvironmentVariable("AdminId") ?? "0");
    }
}