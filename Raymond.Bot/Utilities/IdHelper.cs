using Raymond.Data.Models;

namespace Raymond.Bot.Utilities;

public static class IdPrefixes
{
    public const string Leaderboard = "leaderboard";
    public const string BirthdayList = "birthdaylist";
    public const string Identities = "identities";
    public const string CoinFlip = "coinflip";
    public const string WheelSpin = "wheelspin";
    public const string WheelList = "wheellist";
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

public sealed record WheelSpinId(
    int WheelId,
    int SpinCount,
    string OptionSpun,
    string LastOptionSpun,
    bool RemoveOption)
{
    public string Prefix => IdPrefixes.WheelSpin;
    
    public string Build() => $"{Prefix}{CustomId.Separator}{WheelId}{CustomId.Separator}{SpinCount}{CustomId.Separator}{OptionSpun}{CustomId.Separator}{LastOptionSpun}{CustomId.Separator}{RemoveOption}";

    public static bool TryParse(string id, out WheelSpinId? result)
    {
        result = null;

        var parts = id.Split(CustomId.Separator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 6) return false;
        if (parts[0] != IdPrefixes.WheelSpin) return false;

        var wheelId = int.Parse(parts[1]);
        var spinCount = int.Parse(parts[2]);
        var optionSpun = parts[3];
        var lastOptionSpun = parts[4];
        var removeOption = bool.Parse(parts[5]);
        
        result = new WheelSpinId(wheelId, spinCount, optionSpun, lastOptionSpun, removeOption);
        return true;
    }
}

public sealed record WheelOptionListId(int WheelId, string PageToken)
{
    public string Prefix => IdPrefixes.WheelList;
    
    public string Build() => $"{Prefix}{CustomId.Separator}{WheelId}{CustomId.Separator}{PageToken}";

    public static bool TryParse(string id, out WheelOptionListId? result)
    {
        result = null;

        var parts = id.Split(CustomId.Separator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3) return false;
        if (parts[0] != IdPrefixes.WheelList) return false;
        
        var wheelId = int.Parse(parts[1]);
        var pageToken = parts[2];
        
        result = new WheelOptionListId(wheelId, pageToken);
        return true;
    }
}