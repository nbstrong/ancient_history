using System.Text.Json;
using WurmStyleGame.Shared.Types;

namespace WurmStyleGame.Shared.Protocol;

public sealed record ProtocolVersion(ushort Major, ushort Minor);

public sealed record MessageEnvelope<TPayload>(
    ProtocolVersion ProtocolVersion,
    string MessageType,
    string StreamId,
    StreamSequence Sequence,
    CorrelationId CorrelationId,
    DateTimeOffset SentAt,
    TPayload Payload);

public sealed record MutationCommand(
    ActorId ActorId,
    IdempotencyKey IdempotencyKey,
    string RequestHash,
    string MutationType,
    IReadOnlyList<string> TargetWorldCorners,
    IReadOnlyList<int> Deltas);

public static class MessageCodec
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize<TPayload>(MessageEnvelope<TPayload> envelope)
    {
        return JsonSerializer.Serialize(envelope, Options);
    }

    public static MessageEnvelope<TPayload>? Deserialize<TPayload>(string json)
    {
        return JsonSerializer.Deserialize<MessageEnvelope<TPayload>>(json, Options);
    }
}
