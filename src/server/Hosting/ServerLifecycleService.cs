using Microsoft.Extensions.Options;

namespace WurmStyleGame.Server.Hosting;

public sealed class ServerLifecycleService(
    IReadinessState readiness,
    IOptions<ServerOptions> options,
    ILogger<ServerLifecycleService> logger) : IHostedLifecycleService
{
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        ServerOptions server = options.Value;
        logger.LogInformation(
            "World server starting. InstanceId: {InstanceId}; Environment: {EnvironmentName}",
            server.InstanceId,
            server.EnvironmentName);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        readiness.RemoveReason(ReadinessState.StartupReason);
        logger.LogInformation(
            "World server started. InstanceId: {InstanceId}",
            options.Value.InstanceId);
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        readiness.AddReason(ReadinessState.ShutdownReason);
        logger.LogInformation(
            "World server stopping. InstanceId: {InstanceId}",
            options.Value.InstanceId);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "World server stopped. InstanceId: {InstanceId}",
            options.Value.InstanceId);
        return Task.CompletedTask;
    }
}
