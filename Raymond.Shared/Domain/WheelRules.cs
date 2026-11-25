using Raymond.Shared.Entities;

namespace Raymond.Shared.Domain;

public class WheelRules
{
    public const int MaxWheels = 25;
    public const int MaxOptionLength = 50;
    public const int MaxFileBytes = 50_000;

    public List<string> ParseOptions(string fileContent)
    {
        return fileContent
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    public bool IsValidOption(string option) => option.Length <= MaxOptionLength;

    public bool HasOption(WheelPicker wheel, string option)
        => wheel.Options.Any(o =>
            string.Equals(o.Option.Trim(), option.Trim(), StringComparison.CurrentCultureIgnoreCase));

    /// <returns>An error message if the wheel cannot be created, otherwise null</returns>
    public string? ValidateWheelCreation(List<WheelPicker> wheels, string name)
    {
        if (wheels.Count >= MaxWheels)
        {
            return $"Maximum number of wheels reached in the server ({MaxWheels}). Please delete one to add another";
        }

        if (wheels.Any(x => x.Name == name))
        {
            return "A wheel with that name already exists";
        }

        return null;
    }

    public bool IsOptionsFile(string? fileName)
        => fileName is not null && fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

    public bool IsWithinFileLimit(long fileSize) => fileSize <= MaxFileBytes;

    public WheelOption PickOption(WheelPicker wheel, Random random)
        => wheel.AvailableOptions[random.Next(wheel.AvailableOptions.Count)];
}
