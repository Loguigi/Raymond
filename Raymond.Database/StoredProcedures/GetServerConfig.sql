create or alter proc [GetServerConfig]

@GuildId bigint

as
begin
    select * from ServerConfigs where GuildId = @GuildId
end
go