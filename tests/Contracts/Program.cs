using WurmStyleGame.Server.Actions;
using WurmStyleGame.Server.Persistence.Repositories;
using WurmStyleGame.Shared.Protocol;
using WurmStyleGame.Shared.Types;
using System.Text.RegularExpressions;

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

string FindRepositoryRoot()
{
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "wurm-style-game.sln")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate the repository root.");
}

string repositoryRoot = FindRepositoryRoot();
string projectText = File.ReadAllText(Path.Combine(repositoryRoot, "project.godot"));
string sceneText = File.ReadAllText(Path.Combine(repositoryRoot, "src/client/Scenes/Main.tscn"));
string clientProjectText = File.ReadAllText(Path.Combine(repositoryRoot, "AncientHistory.Client.csproj"));
string clientSourceRoot = Path.Combine(repositoryRoot, "src/client");

Check(
    projectText.Contains("run/main_scene=\"res://src/client/Scenes/Main.tscn\"", StringComparison.Ordinal),
    "Godot main scene references Main.tscn");
Check(
    sceneText.Contains(
        "[ext_resource type=\"Script\" path=\"res://src/client/Scripts/Bootstrap.cs\"",
        StringComparison.Ordinal),
    "Main scene references Bootstrap.cs");

string[] requiredSceneNodes =
[
    "[node name=\"Main\" type=\"Control\"]",
    "[node name=\"CenterContainer\" type=\"CenterContainer\" parent=\".\"]",
    "[node name=\"VBoxContainer\" type=\"VBoxContainer\" parent=\"CenterContainer\"]",
    "[node name=\"TitleLabel\" type=\"Label\" parent=\"CenterContainer/VBoxContainer\"]",
    "[node name=\"StatusLabel\" type=\"Label\" parent=\"CenterContainer/VBoxContainer\"]",
    "[node name=\"BuildLabel\" type=\"Label\" parent=\"CenterContainer/VBoxContainer\"]",
];

foreach (string requiredSceneNode in requiredSceneNodes)
{
    Check(sceneText.Contains(requiredSceneNode, StringComparison.Ordinal), $"Scene contains {requiredSceneNode}");
}

foreach (Match resourceMatch in Regex.Matches(projectText + sceneText, "res://[^\\\"\\r\\n]+"))
{
    string resourcePath = resourceMatch.Value["res://".Length..].Replace('/', Path.DirectorySeparatorChar);
    Check(File.Exists(Path.Combine(repositoryRoot, resourcePath)), $"Resource exists: {resourceMatch.Value}");
}

Check(
    !clientProjectText.Contains("WurmStyleGame.Server", StringComparison.Ordinal),
    "Client project has no server dependency");

foreach (string clientScriptPath in Directory.EnumerateFiles(
             clientSourceRoot,
             "*.cs",
             SearchOption.AllDirectories))
{
    string relativeScriptPath = Path.GetRelativePath(repositoryRoot, clientScriptPath);
    string uidPath = $"{clientScriptPath}.uid";
    Check(File.Exists(uidPath), $"Client script has UID sidecar: {relativeScriptPath}");

    if (File.Exists(uidPath))
    {
        string uid = File.ReadAllText(uidPath).Trim();
        Check(
            Regex.IsMatch(uid, "^uid://[a-y0-8]+$"),
            $"Client script UID is valid: {relativeScriptPath}");
    }
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

if (failures > 0)
{
    Environment.Exit(1);
}
