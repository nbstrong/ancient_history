namespace WurmStyleGame.Shared.Types;

public readonly record struct ChunkId(string Value);
public readonly record struct ZoneId(string Value);
public readonly record struct EntityId(string Value);
public readonly record struct ActionId(string Value);
public readonly record struct ItemId(string Value);
public readonly record struct ActorId(string Value);
public readonly record struct MutationId(string Value);
public readonly record struct CorrelationId(string Value);
public readonly record struct IdempotencyKey(string Value);

public readonly record struct ChunkRevision(long Value)
{
    public static ChunkRevision Initial => new(0);
}

public readonly record struct StreamSequence(ulong Value)
{
    public static StreamSequence Initial => new(1);
}

public static class IdFactory
{
    public static CorrelationId NewCorrelationId() => new(Guid.NewGuid().ToString("N"));
    public static ActionId NewActionId() => new(Guid.NewGuid().ToString("N"));
    public static MutationId NewMutationId() => new(Guid.NewGuid().ToString("N"));
}
