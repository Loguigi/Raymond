create table MinecraftInfo
(
    GuildId bigint not null primary key,
    ContainerName nvarchar(50) not null,
    Ip nvarchar(100) not null,
    Name nvarchar(200) not null,
    ImageUrl nvarchar(2000) not null,
    MaxPlayers int not null,
    Mods nvarchar(1000) null
)