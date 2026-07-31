# Feature Specification: Authoritative Terrain Core

## Status

Planned. Implementation begins only after Sprint 0 delivery semantics and the single-writer zone skeleton pass their acceptance gates.

## Purpose

Implement the foundational Wurm-style terrain system where elevation is stored per world-space corner, all mutations are authoritative, border ownership is unambiguous, and committed changes recover deterministically after process failure.

## Goals

- Represent surface terrain using integer corner heights as the sole elevation authority.
- Partition terrain into fixed chunks without creating duplicate border authority.
- Support legal corner mutations through typed commands.
- Commit multi-owner mutations atomically.
- Persist snapshots and mutation history.
- Reconstruct exact canonical state after restart.
- Replicate committed snapshots and deltas through ordered streams.

## Non-Goals

- Caves or underground mining.
- Hydrology or simulated water flow.
- Advanced erosion or geology.
- Multi-node zone handoff.
- Final visual quality or terrain shaders.
- Full player progression or tool balance.

## Units and Determinism

- Canonical elevation unit: integer decimeters using signed 32-bit values.
- World tile and corner coordinates: signed integer coordinates.
- Slope and legality checks use integer math.
- Floating-point values are presentation outputs only.
- Any required rounding is explicitly specified and tested.
- Derived normals, slopes, mesh vertices, and collision geometry are never persisted as authority.

## Terrain Geometry

## Chunk Dimensions

Initial chunk dimensions:

- 64 x 64 tiles.
- 65 x 65 logical corner positions.

Invariants:

- `TileCount = TilesX * TilesY`
- `LogicalCornerCount = (TilesX + 1) * (TilesY + 1)`
- Tile index: `localY * TilesX + localX`
- Logical corner index: `localY * (TilesX + 1) + localX`

These formulas are versioned with the snapshot schema.

## Canonical Corner Ownership

Every world-space corner has exactly one canonical owner chunk.

Owner selection must be expressed as a deterministic function of:

- World corner coordinate.
- Chunk dimensions.
- Versioned boundary rule.

The implementation must work for negative world coordinates and exact chunk boundaries.

A recommended rule is that the canonical owner is the chunk immediately northwest of a boundary corner, with a defined world-edge fallback when finite-world boundaries exist. The final rule must be captured in an ADR before implementation and frozen in property tests.

## Borrowed Border and Halo Values

A chunk may expose a complete 65 x 65 logical grid for efficient simulation and client snapshot construction, but not every logical corner is necessarily stored as canonical state in that chunk.

- Canonically owned corners are writable and persisted by the owner.
- Borrowed border values are read caches resolved from neighboring owners.
- Borrowed values are rebuildable and may be discarded.
- A neighbor cache mismatch is an integrity failure, not a conflict between two authorities.
- Mutation commands targeting borrowed values are routed to the canonical owner.

## Tile Metadata

Store tile metadata separately from elevation:

- Surface material.
- Paving or packed state.
- Flags.
- Optional ownership or deed overlay reference.

Metadata revisions may initially share the chunk revision but should remain a distinct logical concern from elevation.

## Domain Types

### `TerrainChunk`

- `ChunkId`
- `ChunkCoordinate`
- `TilesX`
- `TilesY`
- Canonically owned corner elevations.
- Tile materials.
- Tile flags.
- `ChunkRevision`

### `WorldCorner`

- `X`
- `Y`

### `CornerRef`

- `WorldCorner`
- `OwnerChunkId`
- Owner-local coordinate.
- Requesting chunk-local coordinate when relevant.

### `CornerDelta`

- `WorldCorner`
- `DeltaDecimeters`

### `TerrainMutationCommand`

- `ActorId`
- `IdempotencyKey`
- `RequestHash`
- Mutation type.
- Expected revisions for all affected owner chunks.
- Typed corner deltas.

### `TerrainMutationResult`

- `MutationId`
- Applied or rejected status.
- Stable rejection code when rejected.
- Pre- and post-revisions by owner chunk.
- Normalized applied deltas.
- Replication stream and sequence metadata.

## Mutation Primitives

Initial internal primitives:

- `ApplyCornerDelta`
- `ApplyLevelPattern`
- `ApplyFlattenPattern`

The first playable client exposes only simple dig and drop operations. Level and flatten may be implemented as server-generated sequences of legal primitive deltas after basic mutation behavior is proven.

## Validation

Each mutation validates:

- Authenticated actor and active session.
- Actor authority and current zone.
- Idempotency state.
- Command size and bounded delta values.
- Expected owner-chunk revisions.
- Target range and line-of-interaction rules.
- Permission hooks.
- Required tool and action state hooks.
- Resulting corner-height bounds.
- Maximum adjacent-corner slope.
- Structural rules introduced by later systems.

Validation failure produces no domain or outbox change except an auditable stored command result where required by idempotency semantics.

## Authoritative Mutation Flow

1. Decode typed command.
2. Resolve all target world corners.
3. Resolve canonical owner chunks.
4. Route command to the authoritative zone executor.
5. Establish idempotency record.
6. Verify expected revisions.
7. Validate all resulting corner states and affected tiles.
8. Compute normalized owner-chunk deltas and dirty regions.
9. Commit mutation event, owner-chunk revisions, snapshot state changes or mutation rows, stream sequences, command result, and outbox records in one PostgreSQL transaction.
10. Apply the committed result to in-memory owner chunks.
11. Rebuild affected borrowed halos and derived tile data.
12. Publish ordered deltas.

No client-visible success is acknowledged before the durable commit and successful authoritative in-memory application.

## Multi-Owner Atomicity

A mutation may affect one, two, or four canonical owner chunks.

Requirements:

- All affected owners are resolved before mutation.
- Zone or owner authority is acquired in stable order.
- All expected revisions are validated together.
- One PostgreSQL transaction commits all owner changes.
- Failure rolls back every affected owner.
- In-memory apply uses the committed normalized result.
- Recovery can reapply the committed result idempotently.

The design must not depend on two neighboring chunks both writing the same corner.

## Revisions

- Each canonical owner chunk has a monotonic `ChunkRevision`.
- Every committed mutation that changes a chunk increments its revision once.
- A multi-owner mutation may produce different pre- and post-revision pairs per owner.
- Client commands include expected revisions for all owner chunks known to be affected.
- Stale revisions return a typed conflict and current revision metadata where safe.

## Persistence Schema

## `terrain_chunks`

- `chunk_id`
- `chunk_x`
- `chunk_y`
- `tiles_x`
- `tiles_y`
- `revision`
- `snapshot_schema_version`
- `snapshot_blob`
- `snapshot_revision`
- `snapshot_checksum`
- `updated_at`

## `terrain_mutation_events`

- `mutation_id`
- `actor_id`
- `idempotency_key`
- `request_hash`
- `mutation_type`
- `status`
- `created_at`
- `committed_at`

## `terrain_mutation_chunks`

- `mutation_id`
- `chunk_id`
- `pre_revision`
- `post_revision`
- `delta_schema_version`
- `delta_payload`

Required constraints:

- Unique mutation identity.
- Unique actor/idempotency scope through Sprint 0 action history.
- Unique `(chunk_id, post_revision)`.
- Revision continuity validated during write and recovery.

## Snapshot Format

A snapshot contains:

- Schema version.
- Chunk identity and coordinate.
- Dimensions.
- Canonically owned corner elevations.
- Tile material and flags.
- Snapshot revision.
- Checksum.

Borrowed halos and derived values are omitted or explicitly marked as non-authoritative cache sections.

## Recovery

For each owner chunk:

1. Load the newest valid snapshot.
2. Verify checksum and schema support.
3. Read committed mutation chunks where `post_revision > snapshot_revision`.
4. Verify contiguous pre/post revisions.
5. Replay deltas in revision order.
6. Rebuild borrowed halos from canonical neighbors.
7. Recompute derived tile data.
8. Compare final revision with the canonical database row.
9. Fail closed and emit diagnostics on mismatch.

Outbox publication resumes independently from unpublished committed events.

## Replication

### Snapshot

Terrain snapshot message includes:

- Chunk identity and coordinate.
- Snapshot or current chunk revision.
- Complete logical corner grid required by the client, assembled from canonical owners.
- Tile metadata.
- Stream identifier and sequence watermark.
- Payload schema version.

### Delta

Terrain delta includes:

- Mutation identifier.
- Stream identifier and monotonic sequence.
- Owner chunk pre- and post-revisions.
- Changed world corners and resulting authoritative heights or normalized deltas.
- Changed tile metadata when applicable.
- Dedupe key.

The initial implementation should prefer resulting authoritative heights for client application where that simplifies recovery from prediction errors.

## Client Application Rules

- Apply only the next expected stream sequence.
- Drop duplicate or stale deltas.
- Detect a sequence gap and request resynchronization.
- Verify expected chunk revision before applying a delta.
- Rebuild affected mesh and collision regions from authoritative corner data.
- Never apply client-predicted terrain as permanent authority.

## Performance Targets

Initial local test-environment targets:

- Single-owner mutation validate and commit: p95 <= 20 ms.
- Multi-owner mutation touching up to four owners: p95 <= 40 ms.
- In-memory validation and apply excluding database time: p95 <= 5 ms.
- Client delta observation under local two-client test: p95 <= 250 ms.

These are engineering budgets, not player-facing service-level guarantees. Revise them only with measured evidence.

## Observability

Emit at minimum:

- Mutation count by type and result.
- Validation rejection count by reason.
- Owner count per mutation.
- Transaction latency.
- Zone queue wait.
- Mutation apply latency.
- Snapshot save and load latency.
- Replay count and duration.
- Revision-gap count.
- Checksum failure count.
- Halo mismatch count.
- Idempotency replay and conflict count.
- Outbox publication lag.

## Test Plan

## Unit Tests

- Tile and logical-corner index mapping.
- World-to-chunk conversion including negative coordinates.
- Canonical owner resolution.
- Borrowed halo reconstruction.
- Slope calculation and bounds.
- Dirty-region selection.
- Snapshot and delta codecs.

## Property-Based Tests

- Every world corner has exactly one owner.
- Neighbor logical grids agree after halo reconstruction.
- Random legal mutation sequences preserve all invariants.
- Replay from snapshot plus tail equals uninterrupted state.
- Serialization round trips preserve canonical values.

## Integration Tests

- Single-owner mutation persists and reloads.
- Two-owner border mutation commits atomically.
- Four-owner junction mutation commits atomically.
- Stale revision returns conflict.
- Duplicate idempotency key returns original result.
- Outbox event is published after commit.
- Two clients receive and apply ordered deltas.

## Failure-Injection Tests

Inject failure after:

- Idempotency establishment.
- Mutation row creation.
- Owner-chunk update.
- Outbox append.
- Database commit.
- In-memory apply.
- Halo rebuild.
- Publication.
- Snapshot write.
- Snapshot load.
- Mutation replay.

Each failure point must converge to a documented correct state after retry or restart.

## Acceptance Criteria

- Corner heights are the sole elevation source of truth.
- Every world corner has exactly one canonical owner.
- `0` halo or seam mismatches in 10,000 randomized border and junction mutations.
- `0` duplicate mutation applies in retry tests.
- 100/100 crash-injection runs reproduce identical canonical state and revisions.
- Corrupt snapshots are detected and do not silently load.
- Derived terrain values are reproducible from canonical state.
- Two clients converge through snapshot, delta, disconnect, and reconnect tests.
- Mutation latency remains within the initial engineering budget in the documented test environment.

## Delivery Order

1. Pure coordinate, ownership, and terrain math.
2. Property-based ownership and seam tests.
3. Single-writer mutation service.
4. PostgreSQL mutation and snapshot persistence.
5. Crash recovery and replay.
6. Snapshot and delta protocol.
7. Godot terrain rendering.
8. First playable dig/drop command.
9. Multi-client reconnect and resynchronization tests.

## Risks

- Incorrect negative-coordinate division at chunk boundaries.
- Ambiguous corner ownership.
- Treating borrowed halos as persistent competing authority.
- Revision gaps after partial or incorrectly ordered writes.
- Divergence between committed database state and in-memory apply.
- Excessive mesh rebuild regions on the client.

Each risk must have a dedicated automated test before the feature is considered complete.
