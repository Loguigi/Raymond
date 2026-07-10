using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raymond.Data.Repositories;
using Singularity.Bot;
using Singularity.Scheduling;

var builder = Host.CreateApplicationBuilder();

builder.AddDiscordBot(
    Environment.GetEnvironmentVariable("RaymondToken") ?? throw new Exception("Token not set"),
    DiscordIntents.All,
    typeof(Program).Assembly,
    "`",
    typeof(Program).Assembly,
    Environment.GetEnvironmentVariable("RaymondConnectionString") ?? throw new Exception("Connection string not set"));

builder.Services.AddScheduledTasksFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<ServerMemberRepository>();
builder.Services.AddScoped<ServerConfigRepository>();
builder.Services.AddScoped<WheelPickerRepository>();

builder.Services.AddSingleton<Random>();

var host = builder.Build();
await host.RunAsync();