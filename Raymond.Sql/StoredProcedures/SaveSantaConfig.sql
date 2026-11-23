create proc [SaveSantaConfig]

@GuildId bigint,
@SantaRoleId bigint,
@ParticipationDeadline date,
@ExchangeDate date,
@ExchangeLocation nvarchar(200),
@ExchangeAddress nvarchar(max),
@PriceLimit decimal(18, 2),
@LockedIn bit

as
begin
    if exists(select 1 from SantaConfigs where GuildId = @GuildId)
    begin
        update SantaConfigs
        set
            SantaRoleId = @SantaRoleId,
            ParticipationDeadline = @ParticipationDeadline,
            ExchangeDate = @ExchangeDate,
            ExchangeLocation = @ExchangeLocation,
            ExchangeAddress = @ExchangeAddress,
            PriceLimit = @PriceLimit,
            LockedIn = @LockedIn
        where GuildId = @GuildId
    end
    else
    begin
        insert into SantaConfigs
            (GuildId,
             SantaRoleId,
             ParticipationDeadline,
             ExchangeDate,
             ExchangeLocation,
             ExchangeAddress,
             PriceLimit,
             LockedIn)
        values (
                @GuildId,
                @SantaRoleId,
                @ParticipationDeadline,
                @ExchangeDate,
                @ExchangeLocation,
                @ExchangeAddress,
                @PriceLimit,
                @LockedIn)
    end
end
go
