using Microsoft.Extensions.Options;

namespace WurmStyleGame.Server.Hosting;

public static class ServerApplication
{
    public const string DefaultUrl = "http://127.0.0.1:8080";

    public static WebApplication Create(
        string[] args,
        Action<WebApplicationBuilder>? configureBuilder = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
        {
            builder.WebHost.UseUrls(DefaultUrl);
        }

        builder.Services
            .AddOptions<ServerOptions>()
            .Bind(builder.Configuration.GetSection(ServerOptions.SectionName))
            .Configure(options => options.EnvironmentName = builder.Environment.EnvironmentName)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.InstanceId),
                $"{ServerOptions.SectionName}:InstanceId must not be empty.")
            .Validate(
                options => options.ShutdownDrainTimeoutSeconds is >= 1 and <= 60,
                $"{ServerOptions.SectionName}:ShutdownDrainTimeoutSeconds must be between 1 and 60 seconds.")
            .ValidateOnStart();

        builder.Services.AddSingleton<IReadinessState, ReadinessState>();
        builder.Services.AddSingleton<IConfigureOptions<HostOptions>, ConfigureServerHostOptions>();
        builder.Services.AddHostedService<ServerLifecycleService>();

        configureBuilder?.Invoke(builder);

        WebApplication app = builder.Build();
        app.MapHealthEndpoints();
        return app;
    }

    private sealed class ConfigureServerHostOptions(IConfiguration configuration)
        : IConfigureOptions<HostOptions>
    {
        public void Configure(HostOptions options)
        {
            int configuredTimeout = configuration.GetValue<int?>(
                $"{ServerOptions.SectionName}:{nameof(ServerOptions.ShutdownDrainTimeoutSeconds)}") ?? 10;

            // ServerOptions validation reports invalid values during host startup. Keep HostOptions
            // valid until then so the error is the clear, field-specific validation message.
            int effectiveTimeout = configuredTimeout is >= 1 and <= 60 ? configuredTimeout : 10;
            options.ShutdownTimeout = TimeSpan.FromSeconds(effectiveTimeout);
        }
    }
}
