create table SantaParticipants
(
    Id                       int identity
        primary key,
    GuildId                  bigint        not null,
    MemberId                 int           not null,
    GifteeMemberId           int,
    SignificantOtherMemberId int,
    GiftReady                bit default 0 not null
)
go
