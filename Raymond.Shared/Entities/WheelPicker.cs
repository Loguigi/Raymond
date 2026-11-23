namespace Raymond.Shared.Entities;

public record WheelOption(string Option, bool TempRemoved);

public class WheelPicker
{
    public int Id { get; set; }
    public long GuildId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<WheelOption> Options { get; set; } = [];
    public List<WheelOption> AvailableOptions => [.. Options.Where(x => !x.TempRemoved)];
    public List<WheelOption> TempRemovedOptions => [.. Options.Where(x => x.TempRemoved)];
}