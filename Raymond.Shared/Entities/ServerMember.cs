namespace Raymond.Shared.Entities;

public class ServerMember
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public long GuildId { get; set; }
    public string? Name { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public DateOnly? Birthday { get; set; }
    public bool Active { get; set; } = true;
}