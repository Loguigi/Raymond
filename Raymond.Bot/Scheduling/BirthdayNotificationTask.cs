using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Utilities;

namespace Raymond.Bot.Scheduling;

public class BirthdayNotificationTask(
    DiscordClient client,
    ILogger<BirthdayNotificationTask> logger,
    IServerMemberRepository memberRepo,
    IServerConfigRepository configRepo,
    IConfiguration config,
    IOptions<SchedulingOptions> options)
    : BaseMessageSender(client, config), IScheduledTask
{
    private readonly DiscordClient _client = client;
    public string Name => "Birthday Notification";
    public string CronExpression => options.Value.Birthdays;

    public async Task ExecuteTaskRun(CancellationToken ct)
    {
        try
        {
            foreach (var guild in _client.Guilds.Values)
            {
                try
                {
                    var serverConfig = await configRepo.GetConfigAsync(guild, ct);
                    if (!serverConfig.BirthdayNotifications || 
                        TimeZoneHelper.GetCurrentTime(serverConfig.TimeZone) is var nowLocal && nowLocal.Hour != serverConfig.BirthdayTimerHour) continue;
                    
                    var localDate = TimeZoneHelper.GetCurrentDate(serverConfig.TimeZone);
                    var members = await memberRepo.GetMembersAsync(guild, ct);
                    var birthdays = members.Where(member => member.GetDaysUntilBirthday(localDate) <= 14).ToList();

                    foreach (var member in birthdays)
                    {
                        if (member.HasBirthdayToday(localDate))
                        {
                            var m = new M
                            {
                                Content = "@everyone",
                                Embed = new E
                                {
                                    Desc = $"# 🎉 {serverConfig.BirthdayMessage} {member.DisplayMention} (**{member.GetAge(localDate)}**) 🎉",
                                    Color = DiscordColor.Magenta,
                                    Timestamp = true
                                },
                                EveryoneMention = true
                            };
                            
                            await Send(serverConfig.DefaultChannel, m);
                            continue;
                        }

                        var daysUntilBirthday = member.GetDaysUntilBirthday(localDate);
                        var message = daysUntilBirthday switch
                        {
                            1 => $"🎉 {member.DisplayMention}'s birthday is **__tomorrow__**! 🎉",
                            7 or 14 => $"🎉 {member.DisplayMention}'s birthday is in **__{daysUntilBirthday} days__**! 🎉",
                            _ => string.Empty
                        };
                        
                        if (string.IsNullOrWhiteSpace(message)) continue;

                        var e = new E
                        {
                            Desc = message,
                            Color = DiscordColor.Magenta,
                            Footer = member.GetFormattedBirthday(member.GetNextBirthday(localDate))
                        };
                        
                        await Send(serverConfig.DefaultChannel, e);
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