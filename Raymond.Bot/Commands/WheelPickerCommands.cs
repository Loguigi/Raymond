using System.ComponentModel;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raymond.Bot.Discord;
using Raymond.Bot.Utilities;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Commands;

[Command("wheel"), Description("Manage and spin custom wheel pickers"), RequireGuild]
public class WheelPickerCommands(
    DiscordClient client,
    HttpClient http,
    ILogger<WheelPickerCommands> logger,
    IWheelPickerRepository wheelRepo,
    IServerMemberRepository memberRepo,
    Random random)
    : BaseCommand(client, logger)
{
    [Command("create"), Description("Create a new wheel picker"), RequireGuild]
    public async Task CreateWheel(SlashCommandContext ctx,
        [MinMaxLength(1, 100)] string name,
        DiscordAttachment? image=null)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var currentWheels = await wheelRepo.GetAllAsync(ctx.Guild!);
            
            if (currentWheels.Count == wheelRepo.MaxWheels)
            {
                await Send(ctx, Embeds.Error($"Maximum number of wheels reached in the server ({wheelRepo.MaxWheels}). Please delete one to add another"), true);
                return;
            }
            
            if (currentWheels.Any(x => x.Name == name))
            {
                await Send(ctx, Embeds.Error("A wheel with that name already exists"), true);
                return;
            }

            await wheelRepo.SaveWheelAsync(new WheelPicker(ctx.Guild!)
            {
                Name = name,
                ImageUrl = image?.Url,
            });
            
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
            var (hasWheels, wheels) = await TryGetWheels(ctx);
            if (!hasWheels) return;
            
            var wheel = wheels.FirstOrDefault(w => w.Name == wheelName);
            if (wheel == null)
            {
                await Send(ctx, Embeds.Error("Wheel not found"), true);
                return;
            }

            if (file != null)
            {
                // Validate file type
                if (!file.FileName!.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    await Send(ctx, Embeds.Error("Please upload a .txt file"), true);
                    return;
                }

                if (file.FileSize > wheelRepo.MaxFileSizeBytes)
                {
                    await Send(ctx, Embeds.Error("Please upload a file less than 50kb"), true);
                    return;
                }

                try
                {
                    // Download the file content
                    var fileContent = await http.GetStringAsync(file.Url);
        
                    // Parse lines and filter out empty ones
                    var options = fileContent
                        .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .ToList();

                    if (options.Count == 0)
                    {
                        await Send(ctx, Embeds.Error("No valid options found in file"), true);
                        return;
                    }

                    int success = 0;
                    
                    // Add each option to the wheel
                    foreach (var opt in options)
                    {
                        if (opt.Length > 50 || wheel.Options.Any(o =>
                                string.Equals(o.Option.Trim(), opt.Trim(), StringComparison.CurrentCultureIgnoreCase)))
                        {
                            continue;
                        }

                        success++;
                        await wheelRepo.SaveWheelOptionAsync(wheel, new WheelOption(opt, false));
                    }

                    await Send(ctx, Embeds.Success($"Added {success} option(s) to wheel '{wheelName}'"), true);
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
                if (wheel.Options.Any(o => string.Equals(o.Option.Trim(), option.Trim(), StringComparison.CurrentCultureIgnoreCase)))
                {
                    await Send(ctx, Embeds.Error("Option already exists"), true);
                    return;
                }
                
                await wheelRepo.SaveWheelOptionAsync(wheel, new WheelOption(option, false));
                await Send(ctx, Embeds.Success($"Added option '{option}' to wheel '{wheelName}'"), true);
            }
        }, "wheel add");
    }

    [Command("spin"), Description("Spin a wheel picker and get the result"), RequireGuild]
    public async Task Spin(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var wheel = (await wheelRepo.GetAllAsync(ctx.Guild!)).FirstOrDefault(w => w.Name == wheelName);
            if (wheel == null)
            {
                await Send(ctx, Embeds.Error("Wheel not found"), true);
                return;
            }

            if (wheel.AvailableOptions.Count == 0)
            {
                await Send(ctx, Embeds.Error("No options available in this wheel"), true);
                return;
            }

            var member = await memberRepo.GetMemberAsync(ctx.Guild!, ctx.User);
            var result = wheel.AvailableOptions[random.Next(wheel.AvailableOptions.Count)];
            var id = new WheelSpinId(wheel.Id, 1, result.Option, "----", false);

            await Send(ctx, CustomMessages.WheelSpin(member, wheel, id));
        }, "wheel spin");
    }

    [Command("delete"), Description("Delete a wheel picker and all its options"), RequireGuild]
    public async Task DeleteWheel(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var (hasWheels, wheels) = await TryGetWheels(ctx);
            if (!hasWheels) return;
            
            var wheel = wheels.FirstOrDefault(w => w.Name == wheelName);
            if (wheel == null)
            {
                await Send(ctx, Embeds.Error("Wheel not found"), true);
                return;
            }
            
            await wheelRepo.DeleteWheelPicker(wheel);
            await Send(ctx, Embeds.Success($"Wheel '{wheelName}' deleted"));
        }, "wheel delete");
    }

    [Command("reload"), Description("Restore removed options for a wheel picker"), RequireGuild]
    public async Task ReloadWheel(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var (hasWheels, wheels) = await TryGetWheels(ctx);
            if (!hasWheels) return;
            
            var wheel = wheels.FirstOrDefault(w => w.Name == wheelName);
            if (wheel == null)
            {
                await Send(ctx, Embeds.Error("Wheel not found"), true);
                return;
            }

            var removedOptions = wheel.TempRemovedOptions.Count;
            await wheelRepo.RestoreWheelOptionsAsync(wheel);

            await Send(ctx, Embeds.Success($"Restored `{removedOptions}` options for {wheel.Name}"));
        }, "wheel reload");
    }

    [Command("list"), Description("List all options for a wheel picker"), RequireGuild]
    public async Task ListWheelOptions(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>] string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var (hasWheels, wheels) = await TryGetWheels(ctx);
            if (!hasWheels) return;
            
            var wheel = wheels.FirstOrDefault(w => w.Name == wheelName);
            if (wheel == null)
            {
                await Send(ctx, Embeds.Error("Wheel not found"), true);
                return;
            }

            await Send(ctx, CustomMessages.WheelOptionList(wheel, new WheelOptionListId(wheel.Id, "1")));
        }, "wheel list");
    }

    [Command("export"), Description("Export wheel options to a file")]
    public async Task ExportWheelOptions(SlashCommandContext ctx,
        [SlashAutoCompleteProvider<WheelAutoCompleteProvider>]
        string wheelName)
    {
        await ExecuteAsync(ctx, async () =>
        {
            var (hasWheels, wheels) = await TryGetWheels(ctx);
            if (!hasWheels) return;

            var wheel = wheels.FirstOrDefault(w => w.Name == wheelName);
            if (wheel == null)
            {
                await Send(ctx, Embeds.Error("Wheel not found"), true);
                return;
            }

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

    private async Task<(bool HasWheels, List<WheelPicker> Wheels)> TryGetWheels(SlashCommandContext ctx)
    {
        var wheels = await wheelRepo.GetAllAsync(ctx.Guild!);
        var hasWheels = wheels.Count != 0;

        if (!hasWheels)
        {
            await Send(ctx, Embeds.Error($"No wheels found in {ctx.Guild!.Name}. Create one using `/wheel create`"), true);
        }

        return (hasWheels, wheels);
    }
}

public class WheelAutoCompleteProvider : IAutoCompleteProvider
{
    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var wheelManager = context.ServiceProvider.GetRequiredService<SqlWheelPickerRepository>();
        var wheels = await wheelManager.GetAllAsync(context.Guild!);

        var input = context.UserInput?.ToLower() ?? string.Empty;

        return wheels
            .Where(w => w.Name.ToLower().Contains(input))
            .Take(25)
            .Select(w => new DiscordAutoCompleteChoice($"{w.Name} - {w.Options.Count} Options", w.Name));
    }
}