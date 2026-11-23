create table WheelPickers
(
    Id       int identity
        primary key,
    GuildId  bigint       not null,
    Name     nvarchar(50) not null,
    ImageUrl nvarchar(2000)
)
go