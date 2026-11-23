create proc [SaveMinecraftInfo]

@GuildId bigint,
@ContainerName nvarchar(50),
@Ip nvarchar(100),
@Name nvarchar(200),
@ImageUrl nvarchar(2000),
@MaxPlayers int,
@Mods nvarchar(1000)
    
as
begin 
    if exists(select 1 from MinecraftInfo where GuildId = @GuildId)
    begin
        update MinecraftInfo
        set
            ContainerName = @ContainerName,
            Ip = @Ip,
            Name = @Name,
            ImageUrl = @ImageUrl,
            MaxPlayers = @MaxPlayers,
            Mods = @Mods
        where GuildId = @GuildId
    end
    
    else
    begin 
        insert into MinecraftInfo
        (
         GuildId, 
         ContainerName, 
         Ip,
         Name, 
         ImageUrl,
         MaxPlayers, 
         Mods
        )
        values (@GuildId,
                @ContainerName,
                @Ip,
                @Name,
                @ImageUrl,
                @MaxPlayers,
                @Mods)
    end
end