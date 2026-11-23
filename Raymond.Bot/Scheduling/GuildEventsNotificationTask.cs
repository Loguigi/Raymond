using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Entities;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Scheduling;

public class GuildEventsNotificationTask(
    DiscordClient client,
    ILogger<BirthdayNotificationTask> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    IConfiguration config,
    IOptions<SchedulingOptions> options)
    : BaseMessageSender(client, config), IScheduledTask
{
    private const int GuildEventNotificationHour = 6;
    private readonly DiscordClient _client = client;
    public string Name => "Guild Events Notification";
    public string CronExpression => options.Value.GuildEvents;
    public bool RunOnStartup => false;
    
    public async Task ExecuteTaskRun(CancellationToken ct)
    {
        try
        {
            foreach (var guild in _client.Guilds.Values)
            {
                try
                {
                    var serverConfig = await configRepo.GetConfigAsync(guild.Id.ToDbId(), ct);
                    ArgumentNullException.ThrowIfNull(serverConfig);
                    
                    var localTime = TimeZoneHelper.GetCurrentTime(serverConfig.TimeZone);
                    
                    if (guild.ScheduledEvents.Count == 0 || localTime.Hour != GuildEventNotificationHour)
                    {
                        await Task.CompletedTask;
                        continue;
                    }
                    
                    var filteredEvents = guild.ScheduledEvents.Values.Where(
                        e => (TimeZoneHelper.ConvertToLocalTime(e.StartTime, serverConfig.TimeZone) - localTime).Days <= 14).ToList();
                    if (filteredEvents.Count == 0)
                    {
                        await Task.CompletedTask;
                        continue;
                    }
                    
                    foreach (var guildEvent in filteredEvents)
                    {
                        var localStartTime = TimeZoneHelper.ConvertToLocalTime(guildEvent.StartTime, serverConfig.TimeZone);
                        var daysUntilEvent = (localStartTime - localTime).Days;

                        ServerMember? member = null;
                        if (guildEvent.Creator != null)
                        {
                            member = await memberRepo.GetMemberAsync(guild.Id.ToDbId(), guildEvent.Creator.Id.ToDbId(), ct);
                        }
                        
                        var createdBy = member is null ? string.Empty : $" created by {member.DisplayMention()}";
                        
                        var message = daysUntilEvent switch
                        {
                            1 => $"‼️**{guildEvent.Name}**{createdBy} is **__tomorrow__**! ‼️",
                            7 or 14 => $"❗️**{guildEvent.Name}**{createdBy} is in **__{daysUntilEvent} days__**! ❗",
                            _ => string.Empty
                        };
                        
                        if (string.IsNullOrWhiteSpace(message)) continue;

                        var m = new M
                        {
                            Content = $"https://discord.com/events/{guild.Id}/{guildEvent.Id}",
                            Embed = new E
                            {
                                Desc = message,
                                Color = DiscordColor.Magenta,
                            }
                        };
                        
                        await Send(await guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), m);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error getting scheduled guild events for {guild}", guild.Name);
                    await SendErrorReport(ex, null, guild);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Guild events notification task failed");
            await SendErrorReport(ex, null, null);
            throw;
        }
    }
}