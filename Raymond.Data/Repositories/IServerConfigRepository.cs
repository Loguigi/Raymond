using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Raymond.Data.Sql;
using Raymond.Shared.Entities;

namespace Raymond.Data.Repositories;

public interface IServerConfigRepository
{
    Task<ServerConfig> GetConfigAsync(DiscordGuild guild, CancellationToken ct = default);
    Task SaveConfigAsync(ServerConfig config, CancellationToken ct = default);
}

public class SqlServerConfigRepository(
    ILogger<SqlServerConfigRepository> logger, 
    IDbConnectionFactory factory) 
    : BaseSqlRepository(logger, factory), IServerConfigRepository
{
    private sealed class ServerConfigRecord
    {
        public long GuildId { get; set; }
        public string? WelcomeMessage { get; set; }
        public string? LeaveMessage { get; set; }
        public long DefaultChannelId { get; set; }
        public bool BirthdayNotifications { get; set; } = true;
        public int BirthdayTimerHour { get; set; }
        public string BirthdayMessage { get; set; } = "Happy Birthday!";
        public bool EnableLevels { get; set; }
        public string LevelUpMessage { get; set; } = "has leveled up!";
        public long? LoggingChannelId { get; set; }
        public string TimeZoneId { get; set; } = "UTC";
    }

    public async Task<ServerConfig> GetConfigAsync(DiscordGuild guild, CancellationToken ct = default)
    {
        var config = new ServerConfig(guild, guild.GetDefaultChannel()!);
        
        var configRecord = await GetFirstAsync<ServerConfigRecord>("GetServerConfig", new {GuildId = (long)guild.Id});
        
        if (configRecord is null)
        {
            await SaveConfigAsync(config, ct).ConfigureAwait(false);
            configRecord = await GetFirstAsync<ServerConfigRecord>("GetServerConfig", new {GuildId = (long)guild.Id});
        }
        
        config.WelcomeMessage = configRecord!.WelcomeMessage;
        config.LeaveMessage = configRecord.LeaveMessage;
        config.DefaultChannel = await guild.GetChannelAsync((ulong)configRecord.DefaultChannelId);
        config.BirthdayNotifications = configRecord.BirthdayNotifications;
        config.BirthdayTimerHour = configRecord.BirthdayTimerHour;
        config.BirthdayMessage = configRecord.BirthdayMessage;
        config.EnableLevels = configRecord.EnableLevels;
        config.LevelUpMessage = configRecord.LevelUpMessage;
        if (configRecord.LoggingChannelId.HasValue) 
            config.LoggingChannel = await guild.GetChannelAsync((ulong)configRecord.LoggingChannelId.Value);
        config.TimeZoneId = configRecord.TimeZoneId;
        
        return config;
    }

    public async Task SaveConfigAsync(ServerConfig config,  CancellationToken ct = default)
    {
        var record = new ServerConfigRecord
        {
            GuildId = config.GuildId,
            WelcomeMessage = config.WelcomeMessage,
            LeaveMessage = config.LeaveMessage,
            DefaultChannelId = config.DefaultChannelId,
            BirthdayNotifications = config.BirthdayNotifications,
            BirthdayTimerHour = config.BirthdayTimerHour,
            BirthdayMessage = config.BirthdayMessage,
            EnableLevels = config.EnableLevels,
            LevelUpMessage = config.LevelUpMessage,
            LoggingChannelId = config.LoggingChannelId,
            TimeZoneId = config.TimeZoneId,
        };
        
        await ExecuteAsync("SaveServerConfig", record);
    }
}