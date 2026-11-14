// using DSharpPlus;
// using DSharpPlus.Entities;
// using Microsoft.Extensions.Logging;
// using Raymond.Bot.Discord;
// using Raymond.Data.Repositories;
//
// namespace Raymond.Bot.Scheduling;
//
// public class BirthdayNotificationTask(
//     DiscordClient client,
//     ILogger<BirthdayNotificationTask> logger,
//     IServerMemberRepository memberRepo,
//     IServerConfigRepository configRepo)
//     : BaseMessageSender(client), IScheduledTask
// {
//     private readonly DiscordClient _client = client;
//     
//     public string Name => "Birthday Notification";
//     public bool ShouldRun(DateTimeOffset easternNow) => true;
//
//     public async Task RunAsync(DateTimeOffset easternNow, CancellationToken ct)
//     {
//         try
//         {
//             foreach (var guild in _client.Guilds.Values)
//             {
//                 try
//                 {
//                     var config = await configRepo.GetConfigAsync(guild, ct);
//                     if (!config.BirthdayNotifications) continue;
//
//                     if (config.BirthdayTimerHour != easternNow.Hour) continue;
//                     
//                     var members = await memberRepo.GetMembersAsync(guild, ct);
//                     var birthdays = members.Where(member => member.DaysUntilBirthday <= 14).ToList();
//
//                     foreach (var member in birthdays)
//                     {
//                         if (member.HasBirthdayToday)
//                         {
//                             var m = new M
//                             {
//                                 Content = "@everyone",
//                                 Embed = new E
//                                 {
//                                     Desc = $"# 🎉 {config.BirthdayMessage} {member.DisplayMention} (**{member.Age}**) 🎉",
//                                     Color = DiscordColor.Magenta,
//                                     Timestamp = true
//                                 },
//                                 EveryoneMention = true
//                             };
//                             
//                             await Send(config.DefaultChannel, m);
//                             continue;
//                         }
//                         
//                         var message = member.DaysUntilBirthday switch
//                         {
//                             1 => $"🎉 {member.DisplayMention}'s birthday is **__tomorrow__**! 🎉",
//                             7 or 14 => $"🎉 {member.DisplayMention}'s birthday is in **__{member.DaysUntilBirthday} days__**! 🎉",
//                             _ => string.Empty
//                         };
//                         
//                         if (string.IsNullOrWhiteSpace(message)) continue;
//
//                         var e = new E
//                         {
//                             Desc = message,
//                             Color = DiscordColor.Magenta,
//                             Footer = member.FormattedNextBirthday
//                         };
//                         
//                         await Send(config.DefaultChannel, e);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     logger.LogError(ex, "Error getting birthdays for {guild}", guild.Name);
//                     await SendErrorReport(ex, null, guild);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             logger.LogCritical(ex, "Birthday notifications error");
//             await SendErrorReport(ex, null, null);
//         }
//     }
// }