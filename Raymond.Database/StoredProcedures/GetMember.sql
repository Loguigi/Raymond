create proc [GetMember]

@GuildId bigint,
@UserId bigint

as
begin
    select * from ServerMembers where GuildId = @GuildId and UserId = @UserId
end
go