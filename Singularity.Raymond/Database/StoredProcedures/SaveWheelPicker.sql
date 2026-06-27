create proc [SaveWheelPicker]

@Id int,
@GuildId bigint,
@Name nvarchar(50),
@ImageUrl nvarchar(2000)

as
begin
    if exists(select 1 from WheelPickers where Id = @Id)
    begin
        update WheelPickers set Name = @Name, ImageUrl = @ImageUrl where Id = @Id
    end

    else
    begin
        insert into WheelPickers (GuildId, Name, ImageUrl) values (@GuildId, @Name, @ImageUrl)
    end
end
go
