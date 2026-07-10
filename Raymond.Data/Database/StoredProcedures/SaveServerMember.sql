create proc [SaveServerMember]

@Id int,
@GuildId bigint,
@UserId bigint,
@Name nvarchar(50),
@Level int,
@Experience int,
@Birthday datetime,
@Active bit

as
begin
    if exists(select 1 from ServerMembers where Id = @Id)
begin
    update ServerMembers 
    set 
        GuildId = @GuildId, 
        UserId = @UserId, 
        Name = @Name, 
        Level = @Level, 
        Experience = @Experience, 
        Birthday = @Birthday, 
        Active = @Active 
    where Id = @Id
end

else
begin
    insert into ServerMembers 
        (GuildId, 
         UserId, 
         Name, 
         Level, 
         Experience, 
         Birthday, 
         Active) 
    values (
            @GuildId,
            @UserId, 
            @Name, 
            @Level, 
            @Experience, 
            @Birthday, 
            @Active)
end
end
go

