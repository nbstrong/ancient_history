using WurmStyleGame.Server.Actions;

namespace WurmStyleGame.Server.Persistence.Repositories;

public interface IOutboxRepository
{
    Task Append(OutboxEvent e, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxEvent>> ReadUnpublished(string streamId, int limit, CancellationToken cancellationToken = default);
    Task MarkPublished(Guid outboxId, DateTimeOffset publishedAt, CancellationToken cancellationToken = default);
}

public sealed class InMemoryOutboxRepository : IOutboxRepository
{
    private readonly List<OutboxEvent> _events = [];

    public Task Append(OutboxEvent e, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _events.Add(e);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxEvent>> ReadUnpublished(string streamId, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<OutboxEvent> result = _events
            .Where(e => e.StreamId == streamId && e.PublishedAt is null)
            .OrderBy(e => e.Sequence.Value)
            .Take(limit)
            .ToList();
        return Task.FromResult(result);
    }

    public Task MarkPublished(Guid outboxId, DateTimeOffset publishedAt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int index = _events.FindIndex(e => e.OutboxId == outboxId);
        if (index < 0)
        {
            return Task.CompletedTask;
        }

        OutboxEvent existing = _events[index];
        _events[index] = existing with { PublishedAt = publishedAt };
        return Task.CompletedTask;
    }
}

public interface IIdempotencyStore
{
    Task<IdempotencyResult?> TryGet(string actorId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task Put(string actorId, string idempotencyKey, string requestHash, string resultPayload, CancellationToken cancellationToken = default);
}

public sealed record IdempotencyResult(string RequestHash, string ResultPayload);

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly Dictionary<(string ActorId, string Key), IdempotencyResult> _records = [];

    public Task<IdempotencyResult?> TryGet(string actorId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records.TryGetValue((actorId, idempotencyKey), out IdempotencyResult? value);
        return Task.FromResult(value);
    }

    public Task Put(string actorId, string idempotencyKey, string requestHash, string resultPayload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records[(actorId, idempotencyKey)] = new IdempotencyResult(requestHash, resultPayload);
        return Task.CompletedTask;
    }
}
