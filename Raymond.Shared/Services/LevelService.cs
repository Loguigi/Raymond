using Raymond.Shared.Entities;

namespace Raymond.Shared.Services;

public class LevelService
{
    private const int ExpGain = 1;
    
    public int ExpNeededForNextLevel(int level) => 2 * (int)Math.Pow(level + 1, 2) - 2;
    
    public bool GrantExp(ServerMember member)
    {
        bool leveledUp = false;

        member.Experience += ExpGain;
        int newLevel = (int)Math.Sqrt((double)member.Experience / 2 + 1);
        if (newLevel > member.Level) 
        {
            member.Level++;
            leveledUp = true;
        }
        
        return leveledUp;
    }

    /// <summary>
    /// Orders a list of members by their level and experience and stores their rankings
    /// </summary>
    /// <param name="members">List of members</param>
    /// <returns>Sorted dictionary</returns>
    public SortedDictionary<int, ServerMember> GetLevelRankings(List<ServerMember> members)
    {
        SortedDictionary<int, ServerMember> rankings = [];
        
        List<ServerMember> serverMembers =
        [
            .. members
                .Where(x => x.Active)
                .OrderByDescending(x => x.Level)
                .ThenByDescending(y => y.Experience)
        ];
        
        for (var i = 1; i <= serverMembers.Count; i++)
        {
            rankings.TryAdd(i, serverMembers[i - 1]);
        }

        return rankings;
    }
}