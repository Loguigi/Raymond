create proc [DeleteSantaConfig]

@GuildId bigint

as
begin
    delete from SantaParticipants
    where GuildId = @GuildId

    delete from SantaConfigs
    where GuildId = @GuildId
end
go
