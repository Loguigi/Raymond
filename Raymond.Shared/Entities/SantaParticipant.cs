namespace Raymond.Shared.Entities;

public class SantaParticipant(ServerMember member)
{
    public ServerMember Member { get; } = member;
    public ServerMember? Giftee { get; set; }
    public ServerMember? SignificantOther { get; set; }
    public bool GiftReady { get; set; }
}