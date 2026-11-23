create table SantaConfigs
(
    GuildId               bigint          not null
        primary key,
    SantaRoleId           bigint          not null,
    ParticipationDeadline date            not null,
    ExchangeDate          date            not null,
    ExchangeLocation      nvarchar(200)   not null,
    ExchangeAddress       nvarchar(max),
    PriceLimit            decimal(18, 2),
    LockedIn              bit default 0   not null
)
go
