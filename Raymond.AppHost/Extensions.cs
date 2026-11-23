using Aspire.Hosting.Docker;
using Aspire.Hosting.Docker.Resources.ComposeNodes;

namespace Raymond.AppHost;

public static class Extensions
{
    private static void UnlessStopped(DockerComposeServiceResource _, Service service) =>
        service.Restart = "unless-stopped";

    public static IResourceBuilder<ProjectResource> AddRaymondDiscordBot(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> sql)
    {
        var bot = builder.AddProject<Projects.Raymond_Bot>("raymond-bot");

        var token = builder.AddParameter("RaymondToken", true);
        var adminId = builder.AddParameter("DiscordAdminId", true);

        var timezone = builder.AddParameter("SchedulingTimezone");
        var discordSyncCron = builder.AddParameter("SchedulingDiscordSync");
        var birthdaysCron = builder.AddParameter("SchedulingBirthdays");
        var guildEventsCron = builder.AddParameter("SchedulingGuildEvents");

        return bot
            .WithReference(sql)
            .WithEnvironment("RaymondToken", token)
            .WithEnvironment("AdminId", adminId)
            .WithEnvironment("Scheduling__Timezone", timezone)
            .WithEnvironment("Scheduling__DiscordSync", discordSyncCron)
            .WithEnvironment("Scheduling__Birthdays", birthdaysCron)
            .WithEnvironment("Scheduling__GuildEvents", guildEventsCron)
            .PublishAsDockerComposeService(UnlessStopped);
    }
}