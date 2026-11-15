using Raymond.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddConnectionString("RaymondSql");

builder.AddDockerComposeEnvironment("raymond")
    .ConfigureComposeFile(f => f.Name = "raymond")
    .WithDashboard(dashboard =>
    {
        dashboard
            .WithHostPort(18888)
            .PublishAsDockerComposeService((_, service) =>
            {
                service.Restart = "unless-stopped";
                service.Ports = ["18888:18888"];
            });
    });

builder.AddRaymondDiscordBot(sql);

builder.Build().Run();
