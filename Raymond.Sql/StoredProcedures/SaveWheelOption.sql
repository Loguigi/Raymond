create proc [SaveWheelOption]

@WheelId int,
@Option nvarchar(50),
@TempRemoved bit

as
begin
    if exists(select 1 from WheelOptions where WheelId = @WheelId and [Option] = @Option)
    begin
        update WheelOptions set TempRemoved = @TempRemoved where WheelId = @WheelId and [Option] = @Option
    end

    else
    begin
        insert into WheelOptions (WheelId, [Option], TempRemoved) values (@WheelId, @Option, @TempRemoved)
    end
end
go

