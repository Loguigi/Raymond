using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raymond.Bot;
using Raymond.Bot.Discord;
using Raymond.Data;
using Raymond.Shared.Domain;
using Raymond.Shared.Extensions;
using Raymond.Shared.Services;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

builder.Services.AddHttpClient<HttpClient>();

builder.Services.AddDiscordBot(
    builder.Configuration["RaymondToken"] ?? throw new Exception("Bot token not configured"),
    DiscordIntents.All,
    typeof(Program).Assembly,
    "`",
    typeof(Program).Assembly);
builder.Services.AddHostedService<BotHostedService>();
builder.Services.AddBotLogging();
builder.Services.AddDataRepositories();
builder.Services.AddSharedServices();
builder.Services.AddTaskScheduler();


builder.Services.AddScoped<DockerService>();
builder.Services.AddSingleton<Random>();

var host = builder.Build();
await host.RunAsync();