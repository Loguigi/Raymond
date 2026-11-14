create or alter proc [GetMinecraftInfo]

@GuildId bigint

as
begin 
    select distinct top 1 * from MinecraftInfo where GuildId = @GuildId
end