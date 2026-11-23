create table ServerMembers
(
    Id         int identity
        primary key,
    GuildId    bigint        not null,
    UserId     bigint        not null,
    Name       nvarchar(50),
    Level      int default 1 not null,
    Experience int default 0 not null,
    Birthday   date,
    Active     bit default 1
)
go