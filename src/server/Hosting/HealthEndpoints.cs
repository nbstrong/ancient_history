using Microsoft.Extensions.Options;

namespace WurmStyleGame.Server.Hosting;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health/live", (IOptions<ServerOptions> options) =>
            Results.Ok(new LivenessResponse(
                "live",
                options.Value.InstanceId,
                DateTimeOffset.UtcNow)));

        endpoints.MapGet("/health/ready", (IOptions<ServerOptions> options, IReadinessState readiness) =>
        {
            IReadOnlyCollection<string> reasons = readiness.Reasons;
            var response = new ReadinessResponse(
                reasons.Count == 0 ? "ready" : "not_ready",
                options.Value.InstanceId,
                reasons);

            return reasons.Count == 0
                ? Results.Ok(response)
                : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        return endpoints;
    }

    private sealed record LivenessResponse(string Status, string InstanceId, DateTimeOffset UtcTime);

    private sealed record ReadinessResponse(
        string Status,
        string InstanceId,
        IReadOnlyCollection<string> Reasons);
}
