# Feature Document: Step 1 Terrain Core (v2)

## Feature Name

Authoritative Corner-Height Terrain Core

## Purpose

Implement the foundational terrain system for Wurm-style terraforming where elevation is stored per corner, all edits are authoritative on the server, and changes persist safely across restarts.

## Background

This project requires Wurm-style terrain behavior. The critical difference from common heightmaps is that tile shape comes from four corner heights, not a single tile height. All gameplay systems that modify world shape depend on this model.

## Goals

- Represent terrain using corner heights as source of truth.
- Support legal corner mutations for terraforming actions.
- Guarantee safe updates at chunk borders and shared chunk corners.
- Persist terrain snapshots + mutation history with deterministic recovery.
- Reconstruct derived tile slope/normal data from corner source data.

## Non-Goals (This Step)

- Full cave mining simulation depth.
- Advanced water flow/hydrology.
- Multi-node zone handoff and distributed chunk ownership.
- Full client visual polish.

## Requirements

## Functional Requirements

1) Corner-Based Terrain Representation
- Each surface tile references four corners (`NW`, `NE`, `SW`, `SE`).
- Elevation is stored only in corner arrays.
- Tile slope/normal values are derived, not authoritative.

2) Chunk Model
- Terrain is partitioned into fixed tile chunks.
- Initial dimensions are fixed for this step: `64 x 64` tiles per chunk.
- Required invariants:
  - `TileCount = TilesX * TilesY`
  - `CornerCount = (TilesX + 1) * (TilesY + 1)`
- Chunk contains:
  - Corner elevation array.
  - Tile material/flags array.
  - Per-chunk monotonic revision counter.

3) Terraform Mutation Primitives
- Support internal mutation operations:
  - `ApplyCornerDelta`
  - `ApplyLevelPattern`
  - `ApplyFlattenPattern`
- Each operation validates slope and rule constraints before commit.

4) Border and Shared-Corner Consistency
- Mutations may affect 1, 2, or 4 chunks depending on touched corners.
- Shared-corner ownership model is canonical-owner only:
  - Each world-space corner has a single owner chunk (lowest `(chunk_x, chunk_y)` that contains the corner on its NW-inclusive boundary).
  - Neighbor chunks read mirrored derived values but do not write canonical corner state.
- When non-owner chunks request edits to owner corners, server forwards to owner chunk mutation path.
- Mutations touching multiple owner chunks commit atomically across all affected chunks.
- Failed commits must roll back all affected chunks.

5) Persistence and Recovery
- Save/load chunk snapshots.
- Append mutation log with enough data for audit/replay.
- Recovery from snapshot + mutation tail must produce exact same canonical corner state.

## Non-Functional Requirements

- Server-authoritative writes only.
- Deterministic terrain math for same inputs on all supported platforms.
- Crash-safe persistence sequence.
- Performance target: `p95 <= 10 ms` for single-corner mutation validate+commit on local chunk.
- Performance target: `p95 <= 20 ms` for atomic cross-chunk mutation touching up to 4 chunks.

## Determinism Rules

- Height unit is integer decimeters (`int32`) in canonical storage.
- Slope calculations use integer delta math.
- Any derived float presentation values use deterministic rounding mode: round half away from zero.
- Normals are derived using a fixed algorithm and must never be persisted as authority.

## Data Model (Initial)

## In-Memory

- `TerrainChunk`
  - `ChunkId`
  - `OriginTileX`, `OriginTileY`
  - `TilesX`, `TilesY`
  - `CornerHeights[]`
  - `TileMaterials[]`
  - `TileFlags[]`
  - `Revision`

- `CornerRef`
  - `WorldCornerX`, `WorldCornerY`
  - `OwnerChunkId`
  - `LocalCornerX`, `LocalCornerY`

- `TerrainMutationEvent`
  - `MutationId`
  - `ActorId`
  - `IdempotencyKey`
  - `MutationType`
  - `CreatedAt`
  - `Status` (`pending`, `committed`, `failed`)

- `TerrainMutationChunkDelta`
  - `MutationId`
  - `ChunkId`
  - `PreRevision`, `PostRevision`
  - `DeltaPayload`

## Index Mapping Rules

- Tile local index: `tile_index = local_y * TilesX + local_x`
- Corner local index: `corner_index = local_y * (TilesX + 1) + local_x`
- These formulas are invariant and versioned with snapshot schema.

## Persistence Tables (Minimum)

- `terrain_chunks`
  - `chunk_id`, `origin_x`, `origin_y`, `tiles_x`, `tiles_y`, `revision`, `snapshot_blob`, `snapshot_revision`, `updated_at`

- `terrain_mutation_events`
  - `mutation_id`, `actor_id`, `idempotency_key`, `mutation_type`, `status`, `created_at`, `committed_at`

- `terrain_mutation_chunks`
  - `mutation_id`, `chunk_id`, `pre_revision`, `post_revision`, `delta_payload_json`

## Commit and Durability Protocol

1) Acquire chunk locks in deterministic order by `ChunkId`.
2) Validate against `ExpectedChunkRevisions[]`.
3) Insert `terrain_mutation_events(status=pending)` and chunk deltas in one DB transaction.
4) Mark event `committed` in same transaction and fsync/commit DB transaction.
5) Apply committed mutation to in-memory chunks.
6) Acknowledge success to caller only after step 4 and step 5 succeed.
7) Snapshotting is asynchronous and records `snapshot_revision` watermark.

Recovery rule:
- On load, restore chunk snapshot at `snapshot_revision` and replay committed mutation chunks where `post_revision > snapshot_revision` in revision order.

## API Surface (Server Internal)

- `ValidateCornerDelta(CornerRef corner, int delta) -> ValidationResult`
- `MutateCornersAtomically(MutationRequest request) -> MutationResult`
- `RecomputeDerivedTileData(TerrainChunk chunk, DirtyRegion region)`
- `SaveChunkSnapshot(ChunkId chunkId)`
- `LoadChunkSnapshot(ChunkId chunkId) -> TerrainChunk`
- `ReplayCommittedMutations(ChunkId chunkId, long snapshotRevision)`

`MutationRequest` must include:
- `IdempotencyKey`
- `ExpectedChunkRevisions[]`
- `TargetWorldCorners[]`
- `Deltas[]`

## Rules and Constraints

- Max slope delta per adjacent corner pair is configurable.
- Permission checks are required before mutation (deed/ownership hooks).
- Border/shared-corner operations acquire locks in deterministic order.
- On validation failure, no partial state changes.
- Stale `ExpectedChunkRevisions[]` returns conflict without side effects.

## Concurrency and Idempotency

- Per-chunk write lock for local edits.
- Multi-chunk transaction lock for cross-chunk edits.
- Lock ordering by `ChunkId` to prevent deadlocks.
- Idempotency uniqueness scope: `(actor_id, idempotency_key)`.
- Idempotency retention: minimum 24 hours.
- Retry behavior: return original `MutationResult` and do not reapply mutation.

## Telemetry

Emit at minimum:
- `terrain_mutation_count` by mutation type.
- `terrain_validation_reject_count` by reason.
- `terrain_atomic_commit_success_count` / `terrain_atomic_commit_failure_count`.
- `terrain_snapshot_save_latency_ms` / `terrain_snapshot_load_latency_ms`.
- `terrain_mutation_apply_latency_ms`.
- `terrain_lock_wait_ms`.
- `terrain_deadlock_retry_count`.
- `terrain_idempotency_replay_hit_count`.
- `terrain_replay_mismatch_count`.

## Test Plan

## Unit Tests

- Corner indexing and neighbor lookup.
- Slope calculation correctness.
- Validation rule pass/fail boundaries.
- Owner-chunk resolution for world corners.
- Deterministic rounding behavior.

## Integration Tests

- Single-corner mutation persists and reloads.
- Cross-border mutation updates all affected chunks atomically.
- 4-chunk shared-corner mutation commits atomically.
- Recovery from snapshot + mutation replay equals pre-crash state.
- Duplicate idempotency key returns prior result without reapply.

## Failure Tests

- Simulated crash after DB commit but before snapshot write.
- Simulated crash during replay; second restart converges to same state.
- Concurrent edits on same corner resolve with lock + deterministic outcome.
- Lock contention and deadlock retry path under randomized parallel mutations.
- Partial/corrupt snapshot detection with fallback to earlier valid snapshot + replay.

## Property-Based Tests

- Run randomized mutation streams and verify:
  - In-memory state equals replayed-from-snapshot state.
  - Chunk seam corners remain consistent across all neighbors.

## Acceptance Criteria

- Terrain source of truth is corner elevations only.
- `0` seam mismatches in `10,000` randomized shared-border/shared-corner mutations.
- `0` duplicate applies in idempotency retry test suite.
- Restart and replay reproduce identical corner state and revisions in `100/100` crash-injection runs.
- Derived slope/normal data is reproducible from corner state with deterministic algorithm checks passing.

## Rollout Plan

1) Implement in-memory chunk + corner model and index invariants.
2) Implement validation + mutation primitives.
3) Implement owner-chunk mapping and multi-chunk atomic mutation path.
4) Implement persistence with committed mutation events + chunk deltas.
5) Implement snapshot watermark + replay flow.
6) Enable replay/recovery tests and telemetry gates.

## Dependencies

- Shared ID/type definitions.
- Persistence layer bootstrap for terrain tables.
- Basic server tick/update loop.

## Risks

- Incorrect owner-chunk mapping causing seam artifacts.
- Deadlocks from improper multi-chunk lock ordering.
- Drift between derived tile cache and corner source data.

## Open Questions

- Whether caves share identical ownership/indexing rules in Step 1 or follow Step 2 extension.
- Snapshot cadence target after first profiling pass.
