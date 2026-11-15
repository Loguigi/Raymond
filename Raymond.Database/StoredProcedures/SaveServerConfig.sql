create or alter proc [SaveServerConfig]

@GuildId bigint,
@WelcomeMessage nvarchar(200),
@LeaveMessage nvarchar(200),
@DefaultChannelId bigint,
@BirthdayNotifications bit,
@BirthdayTimerHour int,
@BirthdayMessage nvarchar(200),
@EnableLevels bit,
@LevelUpMessage nvarchar(200),
@LoggingChannelId bigint,
@TimeZoneId nvarchar(100)

as
begin
    if exists(select 1 from ServerConfigs where GuildId = @GuildId)
begin
    update ServerConfigs 
    set 
        WelcomeMessage = @WelcomeMessage, 
        LeaveMessage = @LeaveMessage, 
        DefaultChannelId = @DefaultChannelId, 
        BirthdayNotifications = @BirthdayNotifications, 
        BirthdayTimerHour = @BirthdayTimerHour, 
        BirthdayMessage = @BirthdayMessage, 
        EnableLevels = @EnableLevels, 
        LevelUpMessage = @LevelUpMessage, 
        LoggingChannelId = @LoggingChannelId,
        TimeZoneId = @TimeZoneId
    where GuildId = @GuildId
end

else
begin
    insert into ServerConfigs 
        (GuildId, 
         WelcomeMessage, 
         LeaveMessage, 
         DefaultChannelId, 
         BirthdayNotifications, 
         BirthdayTimerHour, 
         BirthdayMessage, 
         EnableLevels, 
         LevelUpMessage, 
         LoggingChannelId) 
    values (
            @GuildId, 
            @WelcomeMessage, 
            @LeaveMessage, 
            @DefaultChannelId, 
            @BirthdayNotifications, 
            @BirthdayTimerHour, 
            @BirthdayMessage, 
            @EnableLevels, 
            @LevelUpMessage, 
            @LoggingChannelId)
end
end
go