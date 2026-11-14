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

        var token = builder.AddParameter("raymond-token", true);

        return bot
            .WithReference(sql)
            .WithEnvironment("RaymondToken", token);
    }
}