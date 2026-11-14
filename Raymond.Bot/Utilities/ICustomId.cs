namespace Raymond.Bot.Utilities;

public interface ICustomId<TSelf> where TSelf : ICustomId<TSelf>
{
    string Prefix { get; }
    string Build();
    static abstract bool TryParse(string id, out TSelf? result);
}

public static class CustomId
{
    public const char Separator = ':';
    
    public static string Build(string prefix, string token)
    {
        return $"{prefix}{Separator}{token}";
    }

    public static bool TryParseTwoPart(string id, string expectedPrefix, out string pageToken)
    {
        pageToken = string.Empty;

        var parts = id.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2) return false;
        if (parts[0] != expectedPrefix) return false;

        pageToken = parts[1];
        return true;
    }
}