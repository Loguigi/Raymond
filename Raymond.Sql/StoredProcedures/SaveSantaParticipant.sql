create proc [SaveSantaParticipant]

@GuildId bigint,
@MemberId int,
@GifteeMemberId int,
@SignificantOtherMemberId int,
@GiftReady bit

as
begin
    if exists(select 1 from SantaParticipants where GuildId = @GuildId and MemberId = @MemberId)
    begin
        update SantaParticipants
        set
            GifteeMemberId = @GifteeMemberId,
            SignificantOtherMemberId = @SignificantOtherMemberId,
            GiftReady = @GiftReady
        where GuildId = @GuildId
          and MemberId = @MemberId
    end
    else
    begin
        insert into SantaParticipants
            (GuildId,
             MemberId,
             GifteeMemberId,
             SignificantOtherMemberId,
             GiftReady)
        values (
                @GuildId,
                @MemberId,
                @GifteeMemberId,
                @SignificantOtherMemberId,
                @GiftReady)
    end
end
go
