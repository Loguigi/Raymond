create proc [SaveSantaCouple]

@GuildId bigint,
@FirstMemberId int,
@SecondMemberId int

as
begin
    update SantaParticipants
    set SignificantOtherMemberId = @SecondMemberId
    where GuildId = @GuildId
      and MemberId = @FirstMemberId

    update SantaParticipants
    set SignificantOtherMemberId = @FirstMemberId
    where GuildId = @GuildId
      and MemberId = @SecondMemberId
end
go
