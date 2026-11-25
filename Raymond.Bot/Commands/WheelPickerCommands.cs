using System.ComponentModel;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Bot.Messages;
using Raymond.Bot.Services;
using Raymond.Bot.Utilities;
using Raymond.Data.Repositories;
using Raymond.Shared.Domain;

namespace Raymond.Bot.Commands;

[Command("wheel"), Description("Manage and spin custom wheel pickers"), RequireGuild]
public class WheelPickerCommands(
    DiscordClient client,
    HttpClient http,
    ILogger<WheelPickerCommands> logger,
    WheelPickerService wheelService,
    WheelRules wheelRules,
    ServerMemberService memberService,
    IConfiguration config)
    : BaseCommand(client, config, logger)
{
    [Command("create"), Description("Create a new wheel picker"), RequireGuild]
    public async Task CreateWheel(SlashCommandContext ctx,
        [MinMaxLength(1, 100)] string name,
        DiscordAttachment? image = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var result = await wheelService.CreateAsync(ctx.Guild!, name, image?.Url);
            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.Success($"**{name}** wheel created! Use `/wheel add` to add options to your wheel."), true);
        }, "wheel create");
    }

    [Command("add"), Description("Add options to a wheel picker, either from a text file or manually"), RequireGuild]
    public async Task AddOption(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName,
        [MinMaxLength(1, 50)] string? option = null,
        DiscordAttachment? file = null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            if (file != null)
            {
                if (!wheelRules.IsOptionsFile(file.FileName))
                {
                    await Send(ctx, Embeds.Error("Please upload a .txt file"), true);
                    return;
                }

                if (!wheelRules.IsWithinFileLimit(file.FileSize))
                {
                    await Send(ctx, Embeds.Error("Please upload a file less than 50kb"), true);
                    return;
                }

                try
                {
                    var fileContent = await http.GetStringAsync(file.Url);
                    var result = await wheelService.AddOptionsFromTextAsync(ctx.Guild!, wheelName, fileContent);
                    if (!result.Succeeded)
                    {
                        await Send(ctx, Embeds.Error(result.Message), true);
                        return;
                    }

                    await Send(ctx, Embeds.Success(result.Message), true);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "Failed to download file from Discord");
                    await Send(ctx, Embeds.Error("Failed to download file"), true);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing file");
                    await Send(ctx, Embeds.Error("Error processing file"), true);
                }
            }
            else if (option != null)
            {
                var result = await wheelService.AddOptionAsync(ctx.Guild!, wheelName, option);
                if (!result.Succeeded)
                {
                    await Send(ctx, Embeds.Error(result.Message), true);
                    return;
                }

                await Send(ctx, Embeds.Success(result.Message), true);
            }
        }, "wheel add");
    }

    [Command("spin"), Description("Spin a wheel picker and get the result"), RequireGuild]
    public async Task Spin(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var result = await wheelService.SpinAsync(ctx.Guild!, wheelName);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            var member = await memberService.GetMemberByUserAsync(ctx.Guild!, ctx.User);
            var id = new WheelSpinId(result.Body.Wheel.Id, 1, result.Body.Option.Option, "----", false);

            await Send(ctx, WheelPickerMessages.WheelSpin(member, ctx.User, result.Body.Wheel, id));
        }, "wheel spin");
    }

    [Command("delete"), Description("Delete a wheel picker and all its options"), RequireGuild]
    public async Task DeleteWheel(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var result = await wheelService.DeleteAsync(ctx.Guild!, wheelName);
            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.Success($"Wheel '{wheelName}' deleted"));
        }, "wheel delete");
    }

    [Command("reload"), Description("Restore removed options for a wheel picker"), RequireGuild]
    public async Task ReloadWheel(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var result = await wheelService.RestoreAsync(ctx.Guild!, wheelName);
            if (!result.Succeeded)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, Embeds.Success(result.Message));
        }, "wheel reload");
    }

    [Command("list"), Description("List all options for a wheel picker"), RequireGuild]
    public async Task ListWheelOptions(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var result = await wheelService.GetWheelAsync(ctx.Guild!, wheelName);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            await Send(ctx, WheelPickerMessages.WheelOptionList(result.Body, ctx.Guild!, new WheelOptionListId(result.Body.Id, "1")));
        }, "wheel list");
    }

    [Command("export"), Description("Export wheel options to a file")]
    public async Task ExportWheelOptions(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>]
        string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var result = await wheelService.GetWheelAsync(ctx.Guild!, wheelName);
            if (!result.Succeeded || result.Body is null)
            {
                await Send(ctx, Embeds.Error(result.Message), true);
                return;
            }

            var wheel = result.Body;
            var options = string.Join(Environment.NewLine, wheel.Options.Select(o => o.Option.ToString()).ToList());

            var m = new M
            {
                Embed = new E
                {
                    Author = ctx.Guild!.Name,
                    AuthorImg = ctx.Guild.IconUrl,
                    Title = wheel.Name,
                    Thumbnail = wheel.ImageUrl,
                    Color = DiscordColor.SapGreen,
                    Desc = $"Exported `{wheel.Options.Count}` options"
                },
                File = new AttachedFile($"{wheel.Name}.txt", new MemoryStream(Encoding.UTF8.GetBytes(options)))
            };

            await Send(ctx, m);
        }, "wheel export");
    }
}

public class WheelAutoCompleteProvider : IAutoCompleteProvider
{
    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var wheelManager = context.ServiceProvider.GetRequiredService<IWheelPickerRepository>();
        var wheels = await wheelManager.GetAllAsync(context.Guild!.Id.ToDbId());

        var input = context.UserInput?.ToLowerInvariant() ?? string.Empty;

        return wheels
            .Where(w => w.Name.ToLowerInvariant().Contains(input))
            .Take(25)
            .Select(w => new DiscordAutoCompleteChoice($"{w.Name} - {w.Options.Count} Options", w.Name));
    }
}
