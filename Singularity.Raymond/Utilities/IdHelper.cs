using Singularity.Raymond.Models;
using Singularity.Shared.Utilities;

namespace Singularity.Raymond.Utilities;

public class IdPrefixes
{
    public const string Leaderboard = "leaderboard";
    public const string BirthdayList = "birthdaylist";
    public const string Identities = "identities";
    public const string CoinFlip = "coinflip";
}

public sealed record LeaderboardId(string PageToken) : ICustomId<LeaderboardId>
{
    public string Prefix => IdPrefixes.Leaderboard;

    public string Build() => CustomId.Build(Prefix, PageToken);

    public static bool TryParse(string id, out LeaderboardId? result)
    {
        result = null;

        if (!CustomId.TryParseTwoPart(id, IdPrefixes.Leaderboard, out var pageToken))
            return false;

        result = new LeaderboardId(pageToken);
        return true;
    }
}

public sealed record BirthdayListId(string PageToken) : ICustomId<BirthdayListId>
{
    public string Prefix => IdPrefixes.BirthdayList;

    public string Build() => CustomId.Build(Prefix, PageToken);

    public static bool TryParse(string id, out BirthdayListId? result)
    {
        result = null;

        if (!CustomId.TryParseTwoPart(id, IdPrefixes.BirthdayList, out var pageToken))
            return false;

        result = new BirthdayListId(pageToken);
        return true;
    }
}

public sealed record IdentitiesId(string PageToken) : ICustomId<IdentitiesId>
{
    public string Prefix => IdPrefixes.Identities;
    public string Build() => CustomId.Build(Prefix, PageToken);

    public static bool TryParse(string id, out IdentitiesId? result)
    {
        result = null;

        if (!CustomId.TryParseTwoPart(id, IdPrefixes.Identities, out var pageToken))
            return false;

        result = new IdentitiesId(pageToken);
        return true;
    }
}

public sealed record CoinFlipId(FlipResult LastFlip, int HeadCount, int TailsCount) : ICustomId<CoinFlipId>
{
    public string Prefix => IdPrefixes.CoinFlip;
    public string Build() => $"{Prefix}{CustomId.Separator}{LastFlip}{CustomId.Separator}{HeadCount}{CustomId.Separator}{TailsCount}";

    public static bool TryParse(string id, out CoinFlipId? result)
    {
        result = null;
        
        var parts = id.Split(CustomId.Separator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4) return false;
        if (parts[0] != IdPrefixes.CoinFlip) return false;

        var lastFlip = Enum.Parse<FlipResult>(parts[1]);
        var headCount = int.Parse(parts[2]);
        var tailsCount = int.Parse(parts[3]);
        
        result = new CoinFlipId(lastFlip, headCount, tailsCount);
        return true;
    }
}