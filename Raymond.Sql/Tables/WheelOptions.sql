create table WheelOptions
(
    WheelId     int           not null
        references WheelPickers,
    [Option]    nvarchar(50)  not null,
    TempRemoved bit default 0 not null
    )
go