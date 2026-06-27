create proc [DeleteWheelPicker]

@Id int

as
begin
    delete from WheelOptions where WheelId = @Id
    delete from WheelPickers where Id = @Id
end
go