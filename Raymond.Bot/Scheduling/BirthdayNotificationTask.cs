using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Services;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Scheduling;

public class BirthdayNotificationTask(
    DiscordClient client,
    ILogger<BirthdayNotificationTask> logger,
    IServerMemberRepository memberRepo,
    BirthdayService birthdayService,
    IServerConfigRepository configRepo,
    IConfiguration config,
    IOptions<SchedulingOptions> options)
    : BaseMessageSender(client, config), IScheduledTask
{
    private readonly DiscordClient _client = client;
    public string Name => "Birthday Notification";
    public string CronExpression => options.Value.Birthdays;
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
                    
                    if (!serverConfig.BirthdayNotifications || 
                        TimeZoneHelper.GetCurrentTime(serverConfig.TimeZone) is var nowLocal && nowLocal.Hour != serverConfig.BirthdayTimerHour) continue;
                    
                    var localDate = TimeZoneHelper.GetCurrentDate(serverConfig.TimeZone);
                    var members = await memberRepo.GetMembersAsync(guild.Id.ToDbId(), ct);
                    var birthdays = members.Where(member => birthdayService.GetDaysUntilBirthday(member, localDate) <= 14).ToList();

                    foreach (var member in birthdays)
                    {
                        if (birthdayService.HasBirthdayToday(member, localDate))
                        {
                            var m = new M
                            {
                                Content = "@everyone",
                                Embed = new E
                                {
                                    Desc = $"# 🎉 {serverConfig.BirthdayMessage} {member.DisplayMention()} (**{birthdayService.GetAge(member, localDate)}**) 🎉",
                                    Color = DiscordColor.Magenta,
                                    Timestamp = true
                                },
                                EveryoneMention = true
                            };
                            
                            await Send(await guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), m);
                            continue;
                        }

                        var daysUntilBirthday = birthdayService.GetDaysUntilBirthday(member, localDate);
                        var message = daysUntilBirthday switch
                        {
                            1 => $"🎉 {member.DisplayMention()}'s birthday is **__tomorrow__**! 🎉",
                            7 or 14 => $"🎉 {member.DisplayMention()}'s birthday is in **__{daysUntilBirthday} days__**! 🎉",
                            _ => string.Empty
                        };
                        
                        if (string.IsNullOrWhiteSpace(message)) continue;

                        var e = new E
                        {
                            Desc = message,
                            Color = DiscordColor.Magenta,
                            Footer = birthdayService.GetNextBirthday(member, localDate).ToDiscordFormat()
                        };
                        
                        await Send(await guild.GetChannelAsync(serverConfig.DefaultChannelId.ToDiscordId()), e);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error getting birthdays for {guild}", guild.Name);
                    await SendErrorReport(ex, null, guild);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Birthday notifications error");
            await SendErrorReport(ex, null, null);
            throw;
        }
    }
}