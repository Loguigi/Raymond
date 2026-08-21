using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace Raymond.Data.Services;

public class DockerService
{
    private readonly DockerClient? _client;
    private readonly ILogger<DockerService> _logger;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        
        try
        {
            _client = new DockerClientBuilder().Build();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize Docker client");
        }
    }
    
    public async Task<string> GetContainerStatusAsync(string containerName)
    {
        var container = await GetContainer(containerName);
        return container is null ? "stopped" : container.State;
    }

    public async Task<bool> TryStartContainerAsync(string containerName)
    {
        if (_client is null) return false;
        
        var container = await GetContainer(containerName);
        if (container is null) return false;
        
        await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
        return true;
    }
    
    public async Task<bool> TryRestartContainerAsync(string containerName)
    {
        if (_client is null) return false;
        
        var container = await GetContainer(containerName);
        if (container is null) return false;

        await _client.Containers.RestartContainerAsync(container.ID, new ContainerRestartParameters());
        
        return true;
    }

    public async Task<string> TryExecuteAsync(string containerName, string command)
    {
        if (_client is null) return "Docker client not running";
        var commands = command.Split(' ');
        
        var container = await GetContainer(containerName);
        if (container is null) return "Container not found";
        
        var exec = await _client.Exec.CreateContainerExecAsync(
            container.ID,
            new ContainerExecCreateParameters
            {
                AttachStdin = false,
                AttachStdout = true,
                AttachStderr = true,
                Cmd = commands
            });

        using var stream = await _client.Exec.StartContainerExecAsync(
            exec.ID,
            new ContainerExecStartParameters
            {
                Detach = false
            });

        var result = await stream.ReadOutputToEndAsync(CancellationToken.None);

        return string.IsNullOrWhiteSpace(result.stdout)
            ? result.stderr
            : result.stdout;
    }
    
    private async Task<ContainerListResponse?> GetContainer(string containerName)
    {
        try
        {
            if (_client is null) return null;
        
            var containers =
                await _client.Containers.ListContainersAsync(new ContainersListParameters
                {
                    All = true
                });

            return containers.FirstOrDefault(c =>
                c.Names.Any(n => string.Equals(n.TrimStart('/'), containerName, StringComparison.OrdinalIgnoreCase)));
        }
        catch
        {
            return null;
        }
    }
}