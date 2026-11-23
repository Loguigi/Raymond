using Microsoft.Extensions.DependencyInjection;
using Raymond.Data.Repositories;
using Raymond.Data.Sql;

namespace Raymond.Data;

public static class Extensions
{
    public static IServiceCollection AddDataRepositories(this IServiceCollection services)
    {
        services.AddTransient<IDbConnectionFactory, SqlConnectionFactory>();
        
        services.AddScoped<IServerMemberRepository, SqlServerMemberRepository>();
        services.AddScoped<IServerConfigRepository, SqlServerConfigRepository>();
        services.AddScoped<IWheelPickerRepository, SqlWheelPickerRepository>();
        services.AddScoped<IMinecraftRepository, SqlMinecraftRepository>();
        services.AddScoped<ISecretSantaRepository, SqlSecretSantaRepository>();

        return services;
    }
}