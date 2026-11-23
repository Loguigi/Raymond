create proc [GetSantaConfig]

@GuildId bigint

as
begin
    select * from SantaConfigs where GuildId = @GuildId
end
go
