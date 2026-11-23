using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;
using Raymond.Shared.Services;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Scheduling;

public class DiscordSyncTask(
    DiscordClient client,
    ILogger<DiscordSyncTask> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    IConfiguration config,
    IOptions<SchedulingOptions> options)
    : BaseMessageSender(client, config), IScheduledTask
{
    private readonly DiscordClient _client = client;
    public string Name => "Discord Sync Task";
    public string CronExpression => options.Value.DiscordSync;
    public bool RunOnStartup => true;

    public async Task ExecuteTaskRun(CancellationToken ct)
    {
        try
        {
            foreach (var guild in _client.Guilds.Values)
            {
                try
                {
                    var serverConfig = await configRepo.GetConfigAsync(guild.Id.ToDbId(), ct);

                    if (serverConfig is null)
                    {
                        serverConfig = new ServerConfig
                        {
                            GuildId = guild.Id.ToDbId(),
                            DefaultChannelId = guild.GetDefaultChannel()!.Id.ToDbId()
                        };

                        await configRepo.SaveConfigAsync(serverConfig, ct);
                    }
                    
                    var serverMembers = await memberRepo.GetMembersAsync(guild.Id.ToDbId(), ct);

                    foreach (var member in guild.Members.Values.Where(m => !m.IsBot))
                    {
                        var serverMember = serverMembers.FirstOrDefault(m => m.UserId.ToDiscordId() == member.Id);
                        if (serverMember is null)
                        {
                            serverMember = new ServerMember
                            {
                                UserId = member.Id.ToDbId(),
                                GuildId = guild.Id.ToDbId(),
                                Active = true
                            };
                            
                            await memberRepo.SaveMemberAsync(serverMember, ct);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error syncing members for {guild}", guild.Name);
                    await SendErrorReport(ex, null, guild);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Discord sync error");
            await SendErrorReport(ex, null, null);
            throw;
        }
    }
}