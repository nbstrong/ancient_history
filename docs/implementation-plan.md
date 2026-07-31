# Implementation Plan

## Purpose

This is the execution plan for the first authoritative, persistent, playable Wurm-style vertical slice.

The plan is organized as reviewable pull requests with explicit dependencies and exit criteria. Work should not advance to the next gate merely because files exist; the required correctness and recovery tests must pass.

## Target Outcome

The first playable outcome is:

1. Two Godot clients connect to one standalone .NET world server.
2. Both subscribe to the same terrain chunk.
3. One client requests a legal corner-height mutation.
4. The server validates and commits the mutation in PostgreSQL.
5. The mutation and replication outbox event commit atomically.
6. Both clients apply the ordered delta.
7. The server restarts.
8. Terrain recovers from snapshot plus mutation history.
9. A reconnecting client receives state identical to the authoritative server.

Everything before this outcome supports that proof. Trees, construction, economy, caves, and distributed scale follow afterward.

## Locked Initial Decisions

- Godot 4.x .NET client.
- Standalone .NET authoritative world server.
- WebSocket transport for the initial slice.
- JSON protocol encoding behind versioned shared contracts.
- PostgreSQL as the only required durable dependency.
- One logical world hosted by one server process initially.
- Internal chunk and zone partitioning.
- Single-writer zone command execution wherever practical.
- Canonical corner ownership with rebuildable border caches.
- Transactional outbox and idempotent command handling.

Changes to these decisions require an ADR.

## Proposed Repository Layout

```text
src/
  client/
    AncientHistory.Client.csproj
    Scenes/
      Main.tscn
      World.tscn
    Scripts/
      Bootstrap.cs
      NetClient.cs
      TerrainView.cs
      TerrainMeshBuilder.cs
      ActionUI.cs
  server/
    AncientHistory.Server.csproj
    Program.cs
    Hosting/
      ServerOptions.cs
      HealthEndpoints.cs
    Sessions/
      SessionGateway.cs
      SessionRegistry.cs
    World/
      WorldServer.cs
      WorldClock.cs
      ZoneIdResolver.cs
      ZoneExecutor.cs
      ZoneManager.cs
    Terrain/
      TerrainChunk.cs
      TerrainMath.cs
      TerrainOwnership.cs
      TerrainService.cs
      TerrainSnapshotCodec.cs
    Actions/
      ActionDefinition.cs
      ActionContext.cs
      ActionRunner.cs
      ActionReservation.cs
      Implementations/
    Entities/
      TreeInstance.cs
      BuildPart.cs
      Container.cs
    Persistence/
      Db.cs
      Migrations/
      Repositories/
    Net/
      ProtocolDispatcher.cs
      ReplicationService.cs
      OutboxPublisher.cs
  shared/
    AncientHistory.Shared.csproj
    Protocol/
      Envelopes.cs
      Commands.cs
      Results.cs
      Snapshots.cs
      Deltas.cs
    Types/
      Ids.cs
      Coordinates.cs
      Revisions.cs
      ErrorCodes.cs
tests/
  Unit/
  Property/
  Integration/
  FailureInjection/
  Load/
docs/
  adr/
```

Names may evolve, but dependency direction is fixed:

- `shared` has no client or server dependency.
- `server` references `shared`.
- `client` references `shared` and Godot.
- Tests may reference any project required by their scope.
- Authoritative domain code must not depend on Godot scene types.

## Pull Request Sequence

## PR 1: Reproducible Toolchain and Bootstrap

### Goal

Make the repository buildable and runnable without developer-specific paths.

### Changes

- Pin the supported .NET SDK with `global.json`.
- Pin and document the supported Godot .NET version.
- Replace the hardcoded Godot executable path with `GODOT_BIN` or PATH lookup.
- Add a root README with bootstrap and run commands.
- Create an executable standalone server project.
- Create a minimal Godot client project and startup scene.
- Add `docker-compose.yml` or equivalent for PostgreSQL development.
- Add migration tooling.
- Add CI for:
  - `dotnet restore`
  - `dotnet build`
  - automated tests
  - formatting or analyzers
  - headless Godot import/project validation

### Validation

- Clean CI environment builds all projects.
- `dotnet run --project src/server` starts a health endpoint.
- Godot client opens without import or script errors.
- Client can open a WebSocket connection and receive a bootstrap response.
- No launcher contains a machine-relative engine path.

### Stop Condition

Do not implement terrain until this PR is reproducible in CI.

## PR 2: Shared Contract Hardening

### Goal

Replace prototype strings and permissive wrappers with stable boundary contracts.

### Changes

- Define validated identifiers:
  - `ChunkId`
  - `ZoneId`
  - `EntityId`
  - `ActionId`
  - `ItemId`
  - `ActorId`
  - `ContainerId`
  - `MutationId`
  - `CorrelationId`
  - `IdempotencyKey`
- Define coordinate types:
  - `WorldTile`
  - `WorldCorner`
  - `ChunkCoordinate`
  - `LocalTile`
  - `LocalCorner`
- Define revision and sequence types.
- Replace parallel corner and delta arrays with typed records.
- Define typed command results and stable rejection codes.
- Define explicit protocol major/minor compatibility rules.
- Add JSON converters and malformed-input rejection tests.

### Required Contract Shape

```csharp
public readonly record struct WorldCorner(int X, int Y);

public readonly record struct CornerDelta(
    WorldCorner Corner,
    int DeltaDecimeters);

public sealed record TerrainMutationCommand(
    ActorId ActorId,
    IdempotencyKey IdempotencyKey,
    string RequestHash,
    IReadOnlyList<ExpectedChunkRevision> ExpectedRevisions,
    IReadOnlyList<CornerDelta> Deltas);
```

### Validation

- Empty and malformed identifiers are rejected.
- Message round trips preserve all typed values.
- Unknown additive JSON fields are tolerated.
- Unsupported major versions are rejected.
- Parallel-array length mismatch is impossible by construction.

## PR 3: PostgreSQL Persistence Foundation

### Goal

Implement the durable transaction boundary before gameplay state exists.

### Changes

- Add connection and transaction abstractions.
- Add migrations for:
  - `action_history`
  - `action_outbox`
  - `stream_sequences`
- Implement PostgreSQL-backed idempotency repository.
- Implement PostgreSQL-backed outbox repository.
- Implement transactional stream sequence allocation.
- Add integration-test database lifecycle.
- Rename in-memory implementations explicitly as test doubles.

### Idempotency Semantics

- Uniqueness scope: `(actor_id, idempotency_key)`.
- Same hash returns the stored original result.
- Different hash returns `IdempotencyConflict`.
- Concurrent first submissions produce one committed result.
- Minimum retention: 24 hours.

### Validation

- `0/10,000` duplicate side effects in concurrent retry tests.
- `0` side effects for same-key/different-hash tests.
- Database migration from empty succeeds.
- Migration upgrade test from the previous schema succeeds once a previous schema exists.

## PR 4: Outbox Publisher and Replication Streams

### Goal

Prove commit-to-publication reliability independently of terrain.

### Changes

- Implement outbox publisher worker.
- Preserve per-stream order.
- Add publication retries and observable failure state.
- Define client-side stream cursor behavior.
- Implement replay-window interfaces.
- Implement snapshot-required response when replay is unavailable.
- Add a synthetic test stream for end-to-end verification.

### Validation

- Crash after commit and before first publish republishes the event.
- Duplicate publication is ignored by the client cursor.
- Reordered and stale events are rejected.
- Sequence gaps trigger resynchronization.
- Outbox lag and retry metrics are emitted.

## PR 5: World Host and Single-Writer Zone Executor

### Goal

Establish authoritative scheduling before domain mutations are added.

### Changes

- Implement `WorldClock` and configurable tick cadence.
- Implement `ZoneExecutor` with a bounded command queue.
- Route session commands to zone authority.
- Ensure network handlers never mutate domain state directly.
- Add graceful shutdown and queue-drain behavior.
- Add tick duration, queue depth, and command wait metrics.

### Validation

- Commands execute in deterministic queue order.
- No two writers mutate the same zone state concurrently.
- Shutdown rejects or drains commands according to a documented policy.
- Slow-command and queue-overflow behavior is tested.

## PR 6: Pure Terrain Model

### Goal

Implement deterministic terrain structures and math without networking or persistence.

### Changes

- Implement 64 x 64 tile chunks.
- Implement 65 x 65 logical corner addressing.
- Store only canonically owned corners as authoritative state.
- Store tile material and flags separately.
- Implement index formulas and bounds checks.
- Implement owner-chunk resolution.
- Implement borrowed-border or halo reconstruction.
- Implement integer slope and validation math.
- Implement local mutation application and dirty-region calculation.

### Unit and Property Tests

- Tile and corner index round trips.
- Owner resolution for interior, edge, and four-chunk junction corners.
- Negative world coordinates.
- Derived tile neighborhood selection.
- Slope boundary conditions.
- Integer-unit and rounding behavior.
- Randomized state generation and serialization round trips.

### Validation

- `0` owner ambiguities.
- `0` halo mismatches after reconstruction.
- Derived values are reproducible from canonical corner state.

## PR 7: Atomic Terrain Mutation Service

### Goal

Apply single- and multi-owner terrain mutations through the authoritative zone path.

### Changes

- Implement typed mutation validation.
- Resolve affected owner chunks.
- Acquire authority in stable order.
- Validate expected revisions.
- Validate permission and slope hooks.
- Produce a deterministic mutation result.
- Update all affected owner chunks atomically in the zone transaction model.
- Append terrain mutation and outbox records in one PostgreSQL transaction.

### Validation

- Stale revisions return conflict with no changes.
- Validation failure produces no partial state.
- 1-, 2-, and 4-owner mutations commit correctly.
- 10,000 randomized border and junction edits produce zero seam mismatches.
- Duplicate commands return the original result without a second apply.

## PR 8: Terrain Snapshot and Recovery

### Goal

Make terrain restart-safe and auditable.

### Changes

- Add migrations for:
  - `terrain_chunks`
  - `terrain_mutation_events`
  - `terrain_mutation_chunks`
- Implement versioned snapshot codec and checksums.
- Persist snapshot revision watermarks.
- Replay committed mutation tails in revision order.
- Rebuild borrowed halos and derived tile data.
- Add corrupt-snapshot fallback policy.
- Add crash injection points around commit, in-memory apply, snapshot, and replay.

### Validation

- 100/100 crash-injection runs converge to identical canonical state.
- Snapshot plus tail replay equals uninterrupted in-memory state.
- Corrupt snapshot is detected and does not silently load.
- Revision gaps fail closed and emit a diagnostic.

## PR 9: Godot Terrain Snapshot Rendering

### Goal

Display authoritative terrain in the client.

### Changes

- Implement session bootstrap and chunk subscription.
- Deliver terrain snapshot messages.
- Build a client mesh from integer corner heights.
- Separate mesh generation from network application.
- Track stream sequence and chunk revision on the client.
- Add debug display for corner coordinates, heights, and revisions.

### Validation

- Client mesh matches a deterministic fixture.
- Snapshot replacement is safe.
- Duplicate snapshot or stale delta does not regress state.
- Client does not invent canonical terrain values.

## PR 10: First Playable Terrain Mutation

### Goal

Complete the first end-to-end gameplay path.

### Changes

- Add corner selection in Godot.
- Add dig and drop test commands.
- Submit expected revisions and idempotency key.
- Display typed rejection reasons.
- Publish terrain delta after durable commit.
- Apply delta to all subscribed clients.
- Implement reconnect with replay or snapshot fallback.

### Validation

- Two clients observe the same ordered mutation.
- Mutation survives server restart.
- Reconnecting client converges to authoritative state.
- Duplicate submission does not double-apply.
- Illegal slope, stale revision, out-of-range, and unauthorized requests fail without side effects.

### Milestone Gate

Do not begin tree or construction implementation until this PR passes its restart and multi-client tests.

## PR 11: Generic Timed Action Pipeline

### Goal

Provide the reusable server-authoritative lifecycle for Wurm-style actions.

### Changes

- Implement phases:
  - `Validate`
  - `Reserve`
  - `Execute`
  - `Resolve`
  - `Commit`
  - `Replicate`
- Use authoritative deadlines rather than client timers.
- Support cancellation, interruption, range loss, tool loss, and disconnect.
- Persist action history and final result.
- Release reservations deterministically.
- Add action snapshot and delta messages.
- Add client progress UI driven by authoritative events.

### Validation

- Interrupted actions never commit output.
- Restart in each phase produces a valid documented outcome.
- Duplicate start, cancel, and complete messages are idempotent.
- Reserved resources cannot be consumed by a competing action.

## PR 12: Tree Lifecycle Vertical Slice

### Goal

Prove persistent physical resource processing.

### Changes

- Implement tree states:
  - standing
  - felled
  - processed
  - stump
  - removed
- Implement chop, process, and stump-removal actions.
- Persist remaining wood units and state revision.
- Replicate physical state and fall orientation.
- Add Godot visuals for each state.

### Validation

- Felling never creates instant duplicated inventory loot.
- Processing consumes remaining units exactly once.
- Concurrent actions cannot enter impossible transitions.
- 100/100 restart tests preserve the lifecycle.

## PR 13: Construction Vertical Slice

### Goal

Prove persistent multi-player contribution.

### Changes

- Implement one wall-part definition.
- Add planned, framed, partial, and complete states.
- Add a versioned bill of materials.
- Implement material-delivery transactions.
- Implement completion action.
- Add staged client visuals.

### Validation

- Two-player material delivery produces `0/10,000` duplicates.
- Completion is blocked until the bill of materials is satisfied.
- Partial construction survives restart.
- Permissions and ownership are enforced server-side.

## Test Strategy

### Unit Tests

- Pure math and value-object behavior.
- Validation boundaries.
- State-machine transitions.
- Protocol serialization.

### Property Tests

- Terrain owner mapping and seam invariants.
- Random mutation replay equivalence.
- Container graph and inventory conservation.

### Integration Tests

- PostgreSQL transactions and constraints.
- Idempotency and outbox behavior.
- Snapshot and replay.
- Session bootstrap and resynchronization.

### Failure-Injection Tests

Inject failure after:

- Idempotency record creation.
- Domain row updates.
- Outbox append.
- Database commit.
- In-memory apply.
- Publication.
- Snapshot write.
- Snapshot load.
- Mutation replay.

### Load Tests

Load tests begin with correctness-focused synthetic clients:

- Command retries.
- Disconnect and reconnect storms.
- Border terrain contention.
- Outbox backlog.
- Slow client replication queues.

Load targets increase only after correctness gates pass.

## Definition of Done for Each Pull Request

- Functional behavior meets stated validation.
- Relevant unit, integration, property, and failure tests pass.
- Persistence and migration impact is documented.
- Protocol changes include compatibility tests.
- Metrics and structured logs exist for new failure modes.
- No machine-specific paths or undocumented manual setup are introduced.
- Documentation reflects the implemented behavior.

## Deferred Work

Do not pull these into the first playable terrain milestone:

- Caves and underground mining.
- Full movement prediction stack.
- Advanced AI and ecology.
- Broad crafting catalog.
- Player market.
- Settlements and deeds.
- Multi-node zone handoff.
- Redis, Kafka, NATS, or object storage.
- Kubernetes or multi-region infrastructure.

The project advances fastest by completing one durable loop, measuring it, and extending the same invariants to the next gameplay system.
