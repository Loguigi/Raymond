using Dapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Raymond.Data.Models;
using Artifice.Data;

namespace Raymond.Data.Repositories;

public class WheelPickerRepository(
    ILogger<WheelPickerRepository> logger, 
    IDbConnectionFactory factory) 
    : BaseRepository(logger, factory)
{
    public const int MaxWheels = 25;
    public const int MaxFileSizeBytes = 50_000;
    
    private sealed class WheelPickerRecord
    {
        public int Id { get; set; }
        public long GuildId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Option { get; set; } = string.Empty;
        public bool? TempRemoved { get; set; }
    }
    
    public async Task<List<WheelPicker>> GetAllAsync(DiscordGuild guild)
    {
        var wheels = new List<WheelPicker>();
        
        var wheelRecords = await GetAllAsync<WheelPickerRecord>("GetWheelPickers", new DynamicParameters(new { GuildId = (long)guild.Id }));

        foreach (var row in wheelRecords)
        {
            var wheel = wheels.FirstOrDefault(w => w.Id == row.Id);

            if (wheel != null && row is { Option: not null, TempRemoved: not null })
            {
                wheel.Options.Add(new WheelOption(row.Option, row.TempRemoved.Value));
            }
            else
            {
                var newWheel = new WheelPicker(guild)
                {
                    Id = row.Id,
                    Name = row.Name,
                    ImageUrl = row.ImageUrl,
                };
                
                if (row is { Option: not null, TempRemoved: not null })
                {
                    newWheel.Options.Add(new WheelOption(row.Option, row.TempRemoved.Value));
                }
                
                wheels.Add(newWheel);
            }
        }
        
        return wheels;
    }

    public async Task SaveWheelAsync(WheelPicker wheelPicker)
    {
        var param = new
        {
            wheelPicker.Id,
            wheelPicker.GuildId,
            wheelPicker.Name,
            wheelPicker.ImageUrl
        };

        await ExecuteAsync("SaveWheelPicker", param);
    }

    public async Task SaveWheelOptionAsync(WheelPicker wheelPicker, WheelOption option)
    {
        var param = new
        {
            WheelId = wheelPicker.Id,
            option.Option,
            option.TempRemoved
        };

        await ExecuteAsync("SaveWheelOption", param);
    }

    public async Task RestoreWheelOptionsAsync(WheelPicker wheelPicker)
    {
        await ExecuteAsync("RestoreWheelOptions", new { wheelPicker.Id });
    }

    public async Task DeleteWheelPicker(WheelPicker wheelPicker)
    {
        await ExecuteAsync("DeleteWheelPicker", new { wheelPicker.Id });
    }
}

