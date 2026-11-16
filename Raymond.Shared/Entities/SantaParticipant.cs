using DSharpPlus.Entities;

namespace Raymond.Shared.Entities;

public class SantaParticipant(ServerMember member)
{
    public ServerMember Member { get; } = member;
    public DiscordUser? Giftee { get; set; }
    public DiscordUser? SignificantOther { get; set; }
    public bool GiftReady { get; set; }
}