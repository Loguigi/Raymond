using Raymond.Shared.Entities;

namespace Raymond.Shared.Domain;

public class MinecraftRules
{
    /// <summary>
    /// Applies a partial setup over the current server info: omitted values keep
    /// their current value. Mods keeps its current value when it is empty and no
    /// new value was provided, otherwise the new value replaces it.
    /// </summary>
    public MinecraftInfo ApplySetup(
        MinecraftInfo info,
        string? container = null,
        string? ip = null,
        string? name = null,
        string? image = null,
        int? maxPlayers = null,
        string? mods = null)
    {
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

        return info;
    }
}
