create or alter proc [GetWheelPickers]

@GuildId bigint

as
begin
    select
        p.*,
        o.[Option],
        o.TempRemoved
    from WheelPickers p
        left join WheelOptions o on p.Id = o.WheelId
    where p.GuildId = @GuildId
end
go