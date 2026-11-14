create proc [RestoreWheelOptions]

@Id int

as
begin
    update WheelOptions set TempRemoved = 0 where WheelId = @Id
end
go