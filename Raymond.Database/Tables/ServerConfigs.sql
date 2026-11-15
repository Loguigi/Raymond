create table ServerConfigs
(
    GuildId               bigint                                  not null
        primary key,
    WelcomeMessage        nvarchar(200),
    LeaveMessage          nvarchar(200),
    DefaultChannelId      bigint                                  not null,
    BirthdayNotifications bit           default 1                 not null,
    BirthdayTimerHour     int           default 0                 not null,
    BirthdayMessage       nvarchar(200) default 'Happy Birthday!' not null,
    EnableLevels          bit           default 0                 not null,
    LevelUpMessage        nvarchar(200) default 'has leveled up!' not null,
    LoggingChannelId      bigint,
    TimeZoneId            nvarchar(100)  default 'UTC'             not null
)
go