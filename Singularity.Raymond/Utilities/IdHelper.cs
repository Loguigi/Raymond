using Singularity.Shared.Utilities;

namespace Singularity.Raymond.Utilities;

public class IdPrefixes
{
    public const string Leaderboard = "leaderboard";
    public const string BirthdayList = "birthdaylist";
}

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

public sealed record BirthdayListId(string PageToken) : ICustomId<BirthdayListId>
{
    private const string Id = "birthdaylist";

    public string Prefix => Id;

    public string Build() => $"{Prefix}:{PageToken}";

    public static bool TryParse(string id, out BirthdayListId? result)
    {
        result = null;

        var parts = id.Split(':', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 2) return false;
        if (parts[0] != Id) return false;
        
        result = new BirthdayListId(parts[1]);
        return true;
    }
}