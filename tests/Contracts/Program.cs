using WurmStyleGame.Server.Actions;
using WurmStyleGame.Server.Persistence.Repositories;
using WurmStyleGame.Shared.Protocol;
using WurmStyleGame.Shared.Types;

int failures = 0;

void Check(bool condition, string name)
{
    if (!condition)
    {
        Console.Error.WriteLine($"FAIL: {name}");
        failures++;
        return;
    }

    Console.WriteLine($"PASS: {name}");
}

var envelope = new MessageEnvelope<MutationCommand>(
    new ProtocolVersion(1, 0),
    "terrain.mutate",
    "chunk:abc",
    StreamSequence.Initial,
    IdFactory.NewCorrelationId(),
    DateTimeOffset.UtcNow,
    new MutationCommand(
        new ActorId("actor-1"),
        new IdempotencyKey("idem-1"),
        "hash-1",
        "ApplyCornerDelta",
        ["10,20"],
        [1]));

string json = MessageCodec.Serialize(envelope);
MessageEnvelope<MutationCommand>? roundTrip = MessageCodec.Deserialize<MutationCommand>(json);
Check(roundTrip is not null, "Envelope roundtrip non-null");
Check(roundTrip?.ProtocolVersion.Major == 1, "Protocol major preserved");
Check(roundTrip?.Payload.IdempotencyKey.Value == "idem-1", "Idempotency key preserved");

var store = new InMemoryIdempotencyStore();
await store.Put("actor-1", "idem-1", "hash-1", "ok");
IdempotencyResult? idem = await store.TryGet("actor-1", "idem-1");
Check(idem is not null, "Idempotency record stored");
Check(idem?.RequestHash == "hash-1", "Request hash stored");

var outbox = new InMemoryOutboxRepository();
await outbox.Append(new OutboxEvent(
    Guid.NewGuid(),
    "chunk:abc",
    StreamSequence.Initial,
    "terrain.delta",
    "mutation-1",
    "{}",
    DateTimeOffset.UtcNow));
IReadOnlyList<OutboxEvent> unpublished = await outbox.ReadUnpublished("chunk:abc", 10);
Check(unpublished.Count == 1, "Outbox append/read works");

int order = LockOrdering.Compare(
    (LockResourceClass.Chunk, "c2"),
    (LockResourceClass.Entity, "e1"));
Check(order < 0, "Lock primary ordering chunk before entity");

await ServerHostTests.RunAsync(Check);

if (failures > 0)
{
    Environment.Exit(1);
}
