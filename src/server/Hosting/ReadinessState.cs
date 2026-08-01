using System.Collections.Concurrent;

namespace WurmStyleGame.Server.Hosting;

public interface IReadinessState
{
    IReadOnlyCollection<string> Reasons { get; }

    bool AddReason(string reason);

    bool RemoveReason(string reason);
}

public sealed class ReadinessState : IReadinessState
{
    public const string StartupReason = "startup_in_progress";
    public const string ShutdownReason = "shutdown_in_progress";

    private readonly ConcurrentDictionary<string, byte> reasons = new(StringComparer.Ordinal)
    {
        [StartupReason] = 0,
    };

    public IReadOnlyCollection<string> Reasons => reasons.Keys.Order(StringComparer.Ordinal).ToArray();

    public bool AddReason(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return reasons.TryAdd(reason, 0);
    }

    public bool RemoveReason(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return reasons.TryRemove(reason, out _);
    }
}
