create or alter proc [GetServerMembers]

@GuildId bigint

as
begin
    select * from ServerMembers where GuildId = @GuildId
end
go
