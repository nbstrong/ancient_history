using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WurmStyleGame.Server.Hosting;

internal static class ServerHostTests
{
    public static async Task RunAsync(Action<bool, string> check)
    {
        var logs = new RecordingLoggerProvider();
        ShutdownReadinessProbe? shutdownProbe = null;
        WebApplication app = ServerApplication.Create(
            ["--urls", "http://127.0.0.1:0", "--Server:ShutdownDrainTimeoutSeconds", "2"],
            builder =>
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddProvider(logs);
                builder.Services.AddSingleton<ShutdownReadinessProbe>();
                builder.Services.AddSingleton<IHostedService>(services =>
                {
                    var probe = services.GetRequiredService<ShutdownReadinessProbe>();
                    shutdownProbe = probe;
                    return probe;
                });
            });

        IReadinessState readiness = app.Services.GetRequiredService<IReadinessState>();
        check(
            readiness.Reasons.Contains(ReadinessState.StartupReason),
            "Readiness is not ready before startup completes");

        await app.StartAsync();

        string address = GetBoundAddress(app);
        using var client = new HttpClient { BaseAddress = new Uri(address) };

        using HttpResponseMessage liveResponse = await client.GetAsync("/health/live");
        using JsonDocument live = JsonDocument.Parse(await liveResponse.Content.ReadAsStringAsync());
        check(liveResponse.StatusCode == HttpStatusCode.OK, "Liveness returns HTTP 200");
        check(live.RootElement.GetProperty("status").GetString() == "live", "Liveness status is live");
        check(
            live.RootElement.TryGetProperty("instanceId", out JsonElement liveInstanceId) &&
            !string.IsNullOrWhiteSpace(liveInstanceId.GetString()),
            "Liveness includes instance ID");
        check(live.RootElement.EnumerateObject().Count() == 3, "Liveness contains only required fields");
        check(
            live.RootElement.TryGetProperty("utcTime", out JsonElement utcTime) &&
            DateTimeOffset.TryParse(utcTime.GetString(), out DateTimeOffset parsedUtcTime) &&
            parsedUtcTime.Offset == TimeSpan.Zero,
            "Liveness includes a UTC timestamp");

        using HttpResponseMessage readyResponse = await client.GetAsync("/health/ready");
        using JsonDocument ready = JsonDocument.Parse(await readyResponse.Content.ReadAsStringAsync());
        check(readyResponse.StatusCode == HttpStatusCode.OK, "Readiness returns HTTP 200 after startup");
        check(ready.RootElement.GetProperty("status").GetString() == "ready", "Readiness status is ready");
        check(ready.RootElement.GetProperty("reasons").GetArrayLength() == 0, "Ready response has no reasons");
        check(ready.RootElement.EnumerateObject().Count() == 3, "Readiness contains only required fields");
        check(
            ready.RootElement.GetProperty("instanceId").GetString() == liveInstanceId.GetString(),
            "Health endpoints report the same instance ID");

        check(readiness.AddReason("test_dependency_unavailable"), "Readiness reason can be added");

        using HttpResponseMessage unavailableResponse = await client.GetAsync("/health/ready");
        using JsonDocument unavailable = JsonDocument.Parse(await unavailableResponse.Content.ReadAsStringAsync());
        check(
            unavailableResponse.StatusCode == HttpStatusCode.ServiceUnavailable,
            "Readiness returns HTTP 503 with a reason");
        check(
            unavailable.RootElement.GetProperty("status").GetString() == "not_ready",
            "Unavailable readiness status is not_ready");
        check(
            unavailable.RootElement.GetProperty("reasons")[0].GetString() == "test_dependency_unavailable",
            "Readiness returns the machine-readable reason");

        check(readiness.RemoveReason("test_dependency_unavailable"), "Readiness reason can be removed");
        using HttpResponseMessage restoredResponse = await client.GetAsync("/health/ready");
        check(restoredResponse.StatusCode == HttpStatusCode.OK, "Removing final reason restores readiness");

        string instanceId = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value.InstanceId;
        var stopwatch = Stopwatch.StartNew();
        await app.StopAsync();
        stopwatch.Stop();

        check(stopwatch.Elapsed < TimeSpan.FromSeconds(2), "Graceful shutdown completes within drain timeout");
        check(
            readiness.Reasons.Contains(ReadinessState.ShutdownReason),
            "Shutdown transitions readiness to not ready");
        check(
            shutdownProbe?.SawShutdownReason == true,
            "Readiness transitions before hosted services drain");
        check(
            logs.Entries
                .Where(entry => entry.Message.Contains("World server", StringComparison.Ordinal))
                .All(entry => entry.InstanceId == instanceId),
            "All startup and shutdown logs contain the process instance ID");
        check(
            logs.Entries.Any(entry => entry.Message.Contains("stopped", StringComparison.Ordinal)),
            "Shutdown completion is logged");

        await app.DisposeAsync();

        await CheckInvalidDrainTimeoutAsync(check);
    }

    private static async Task CheckInvalidDrainTimeoutAsync(Action<bool, string> check)
    {
        await using WebApplication invalidApp = ServerApplication.Create(
            ["--urls", "http://127.0.0.1:0", "--Server:ShutdownDrainTimeoutSeconds", "0"],
            builder => builder.Logging.ClearProviders());

        try
        {
            await invalidApp.StartAsync();
            check(false, "Invalid drain timeout fails startup");
        }
        catch (OptionsValidationException exception)
        {
            check(
                exception.Message.Contains("between 1 and 60 seconds", StringComparison.Ordinal),
                "Invalid drain timeout fails startup with a clear validation error");
        }
    }

    private static string GetBoundAddress(WebApplication app)
    {
        IServer server = app.Services.GetRequiredService<IServer>();
        IServerAddressesFeature addresses = server.Features.Get<IServerAddressesFeature>()
            ?? throw new InvalidOperationException("The server did not expose a bound address.");

        return addresses.Addresses.Single();
    }

    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger(ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string? instanceId = null;
            if (state is IEnumerable<KeyValuePair<string, object?>> properties)
            {
                instanceId = properties
                    .FirstOrDefault(property => property.Key == "InstanceId")
                    .Value?
                    .ToString();
            }

            entries.Enqueue(new LogEntry(formatter(state, exception), instanceId));
        }
    }

    private sealed record LogEntry(string Message, string? InstanceId);

    private sealed class ShutdownReadinessProbe(IReadinessState readiness) : IHostedService
    {
        public bool SawShutdownReason { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            SawShutdownReason = readiness.Reasons.Contains(ReadinessState.ShutdownReason);
            return Task.CompletedTask;
        }
    }
}
