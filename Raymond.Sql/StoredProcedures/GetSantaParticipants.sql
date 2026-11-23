create proc [GetSantaParticipants]

@GuildId bigint

as
begin
    select sp.MemberId,
           sm.UserId,
           sp.GifteeMemberId,
           sg.UserId as GifteeUserId,
           sp.SignificantOtherMemberId,
           so.UserId as SignificantOtherUserId,
           sp.GiftReady
    from SantaParticipants sp
             join ServerMembers sm on sm.Id = sp.MemberId
             left join ServerMembers sg on sg.Id = sp.GifteeMemberId
             left join ServerMembers so on so.Id = sp.SignificantOtherMemberId
    where sp.GuildId = @GuildId
end
go
