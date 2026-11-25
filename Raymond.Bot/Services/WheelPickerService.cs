using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Domain;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Services;

public record WheelSpinResult(WheelPicker Wheel, WheelOption Option);

public class WheelPickerService(
    IWheelPickerRepository wheelRepo,
    WheelRules wheelRules,
    Random random)
{
    public Task<List<WheelPicker>> GetAllAsync(DiscordGuild guild, CancellationToken ct = default)
        => wheelRepo.GetAllAsync(guild.Id.ToDbId(), ct);

    public async Task<Result<WheelPicker>> CreateAsync(
        DiscordGuild guild,
        string name,
        string? imageUrl,
        CancellationToken ct = default)
    {
        var wheels = await wheelRepo.GetAllAsync(guild.Id.ToDbId(), ct);
        var error = wheelRules.ValidateWheelCreation(wheels, name);
        if (error is not null)
        {
            return Result<WheelPicker>.Failed(error);
        }

        var wheel = new WheelPicker
        {
            GuildId = guild.Id.ToDbId(),
            Name = name,
            ImageUrl = imageUrl,
        };

        await wheelRepo.SaveWheelAsync(wheel, ct);

        return Result<WheelPicker>.Success(wheel);
    }

    public async Task<Result<WheelPicker>> AddOptionAsync(
        DiscordGuild guild,
        string wheelName,
        string option,
        CancellationToken ct = default)
    {
        var found = await FindWheelAsync(guild, wheelName, ct);
        if (!found.Succeeded || found.Body is null)
        {
            return Result<WheelPicker>.Failed(found.Message);
        }

        if (wheelRules.HasOption(found.Body, option))
        {
            return Result<WheelPicker>.Failed("Option already exists");
        }

        await wheelRepo.SaveWheelOptionAsync(found.Body, new WheelOption(option, false), ct);

        return Result<WheelPicker>.Success(found.Body, $"Added option '{option}' to wheel '{wheelName}'");
    }

    public async Task<Result<int>> AddOptionsFromTextAsync(
        DiscordGuild guild,
        string wheelName,
        string fileContent,
        CancellationToken ct = default)
    {
        var found = await FindWheelAsync(guild, wheelName, ct);
        if (!found.Succeeded || found.Body is null)
        {
            return Result<int>.Failed(found.Message);
        }

        var wheel = found.Body;
        var options = wheelRules.ParseOptions(fileContent);

        if (options.Count == 0)
        {
            return Result<int>.Failed("No valid options found in file");
        }

        var added = 0;
        foreach (var option in options)
        {
            if (!wheelRules.IsValidOption(option) || wheelRules.HasOption(wheel, option))
            {
                continue;
            }

            added++;
            await wheelRepo.SaveWheelOptionAsync(wheel, new WheelOption(option, false), ct);
        }

        return Result<int>.Success(added, $"Added {added} option(s) to wheel '{wheelName}'");
    }

    public async Task<Result<WheelSpinResult>> SpinAsync(
        DiscordGuild guild,
        string wheelName,
        CancellationToken ct = default)
    {
        var found = await FindWheelAsync(guild, wheelName, ct);
        if (!found.Succeeded || found.Body is null)
        {
            return Result<WheelSpinResult>.Failed(found.Message);
        }

        var wheel = found.Body;

        if (wheel.AvailableOptions.Count == 0)
        {
            return Result<WheelSpinResult>.Failed("No options available in this wheel");
        }

        return Result<WheelSpinResult>.Success(new WheelSpinResult(wheel, wheelRules.PickOption(wheel, random)));
    }

    public async Task<Result<WheelPicker>> DeleteAsync(
        DiscordGuild guild,
        string wheelName,
        CancellationToken ct = default)
    {
        var found = await FindWheelAsync(guild, wheelName, ct);
        if (!found.Succeeded || found.Body is null)
        {
            return Result<WheelPicker>.Failed(found.Message);
        }

        await wheelRepo.DeleteWheelPicker(found.Body, ct);

        return Result<WheelPicker>.Success(found.Body, $"Wheel '{wheelName}' deleted");
    }

    public async Task<Result<int>> RestoreAsync(
        DiscordGuild guild,
        string wheelName,
        CancellationToken ct = default)
    {
        var found = await FindWheelAsync(guild, wheelName, ct);
        if (!found.Succeeded || found.Body is null)
        {
            return Result<int>.Failed(found.Message);
        }

        var removed = found.Body.TempRemovedOptions.Count;
        await wheelRepo.RestoreWheelOptionsAsync(found.Body, ct);

        return Result<int>.Success(removed, $"Restored `{removed}` options for {found.Body.Name}");
    }

    public Task<Result<WheelPicker>> GetWheelAsync(
        DiscordGuild guild,
        string wheelName,
        CancellationToken ct = default)
        => FindWheelAsync(guild, wheelName, ct);

    private async Task<Result<WheelPicker>> FindWheelAsync(
        DiscordGuild guild,
        string wheelName,
        CancellationToken ct = default)
    {
        var wheels = await wheelRepo.GetAllAsync(guild.Id.ToDbId(), ct);

        if (wheels.Count == 0)
        {
            return Result<WheelPicker>.Failed($"No wheels found in {guild.Name}. Create one using `/wheel create`");
        }

        var wheel = wheels.FirstOrDefault(w => w.Name == wheelName);
        if (wheel is null)
        {
            return Result<WheelPicker>.Failed("Wheel not found");
        }

        return Result<WheelPicker>.Success(wheel);
    }
}
