using Microsoft.Extensions.Logging;
using Raymond.Data.Sql;
using Raymond.Shared.Entities;

namespace Raymond.Data.Repositories;

public interface IWheelPickerRepository
{
    Task<List<WheelPicker>> GetAllAsync(long guildId, CancellationToken ct = default);
    Task SaveWheelAsync(WheelPicker wheelPicker, CancellationToken ct = default);
    Task SaveWheelOptionAsync(WheelPicker wheelPicker, WheelOption option, CancellationToken ct = default);
    Task RestoreWheelOptionsAsync(WheelPicker wheelPicker, CancellationToken ct = default);
    Task DeleteWheelPicker(WheelPicker wheelPicker, CancellationToken ct = default);
}

public class SqlWheelPickerRepository(
    ILogger<SqlWheelPickerRepository> logger, 
    IDbConnectionFactory factory) 
    : BaseSqlRepository(logger, factory), IWheelPickerRepository
{
    private sealed class WheelPickerRecord
    {
        public int Id { get; set; }
        public long GuildId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Option { get; set; } = string.Empty;
        public bool? TempRemoved { get; set; }
    }
    
    public async Task<List<WheelPicker>> GetAllAsync(long guildId, CancellationToken ct = default)
    {
        var wheels = new List<WheelPicker>();
        
        var wheelRecords = await GetAllAsync<WheelPickerRecord>("GetWheelPickers", new { GuildId = guildId });

        foreach (var row in wheelRecords)
        {
            var wheel = wheels.FirstOrDefault(w => w.Id == row.Id);

            if (wheel != null && row is { Option: not null, TempRemoved: not null })
            {
                wheel.Options.Add(new WheelOption(row.Option, row.TempRemoved.Value));
            }
            else
            {
                var newWheel = new WheelPicker
                {
                    GuildId = guildId,
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

    public async Task SaveWheelAsync(WheelPicker wheelPicker, CancellationToken ct = default)
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

    public async Task SaveWheelOptionAsync(WheelPicker wheelPicker, WheelOption option, CancellationToken ct = default)
    {
        var param = new
        {
            WheelId = wheelPicker.Id,
            option.Option,
            option.TempRemoved
        };

        await ExecuteAsync("SaveWheelOption", param);
    }

    public async Task RestoreWheelOptionsAsync(WheelPicker wheelPicker, CancellationToken ct = default)
    {
        await ExecuteAsync("RestoreWheelOptions", new { wheelPicker.Id });
    }

    public async Task DeleteWheelPicker(WheelPicker wheelPicker, CancellationToken ct = default)
    {
        await ExecuteAsync("DeleteWheelPicker", new { wheelPicker.Id });
    }
}

