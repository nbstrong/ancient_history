using WurmStyleGame.Shared.Types;

namespace WurmStyleGame.Server.Actions;

public sealed record OutboxEvent(
    Guid OutboxId,
    string StreamId,
    StreamSequence Sequence,
    string EventType,
    string DedupeKey,
    string PayloadJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt = null);
