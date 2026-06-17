using Singularity.Shared.Utilities;

namespace Singularity.Raymond.Utilities;

public sealed record LeaderboardId(string PageToken) : ICustomId<LeaderboardId>
{
    private const string Id = "leaderboard";

    public string Prefix => Id;

    public string Build() => $"{Prefix}:{PageToken}";

    public static bool TryParse(string id, out LeaderboardId? result)
    {
        result = null;

        var parts = id.Split(':', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 2) return false;
        if (parts[0] != Id) return false;
        
        result = new LeaderboardId(parts[1]);
        return true;
    }
}