# Implementation Plan (Task Board) v2

This plan is the execution order for the first playable Wurm-style vertical slice.

## Goal of This Slice

Prove four capabilities end-to-end:
- Corner-height terrain edits persist and replicate.
- Timed authoritative actions support interruption and resume-safe commit.
- Trees are physical, stateful entities (`standing -> felled -> processed -> stump`).
- Construction progresses part-by-part with explicit material delivery.

Related feature spec:
- `docs/feature-step-1-terrain-core.md`

## Proposed Project Layout

Create this structure under `prjs/wurm-style-game`:

- `src/shared/`
  - `Types/Ids.cs`
  - `Types/Enums.cs`
  - `Math/TerrainMath.cs`
  - `Protocol/Messages.cs`
- `src/server/`
  - `Program.cs`
  - `World/WorldServer.cs`
  - `World/ZoneManager.cs`
  - `Terrain/TerrainChunk.cs`
  - `Terrain/TerrainService.cs`
  - `Terrain/TerrainPersistence.cs`
  - `Actions/ActionDefinition.cs`
  - `Actions/ActionContext.cs`
  - `Actions/ActionRunner.cs`
  - `Actions/ActionLocks.cs`
  - `Actions/ActionOutbox.cs`
  - `Actions/Implementations/ChopTreeAction.cs`
  - `Actions/Implementations/ProcessFelledTreeAction.cs`
  - `Actions/Implementations/RemoveStumpAction.cs`
  - `Actions/Implementations/CreatePlanAction.cs`
  - `Actions/Implementations/DeliverMaterialAction.cs`
  - `Actions/Implementations/CompletePartAction.cs`
  - `Entities/TreeInstance.cs`
  - `Entities/BuildPart.cs`
  - `Entities/Inventory.cs`
  - `Persistence/Db.cs`
  - `Persistence/Repositories/*.cs`
  - `Net/SessionGateway.cs`
  - `Net/ReplicationService.cs`
- `src/client/`
  - `Scenes/World.tscn`
  - `Scripts/NetClient.cs`
  - `Scripts/TerrainView.cs`
  - `Scripts/EntityViewTree.cs`
  - `Scripts/EntityViewBuildPart.cs`
  - `Scripts/ActionUI.cs`
- `tests/`
  - `Unit/TerrainMathTests.cs`
  - `Integration/TerrainPersistenceTests.cs`
  - `Integration/TerrainReplicationTests.cs`
  - `Integration/ActionIdempotencyTests.cs`
  - `Integration/TreeLifecycleTests.cs`
  - `Integration/ConstructionFlowTests.cs`

## Interface Contracts (Mandatory Before Sprint 1 Buildout)

- `ZoneManager` owns authority and lock scope for chunks/entities in a zone.
- Lock ordering is globally fixed: `chunk -> entity -> container`.
- `ActionRunner` contract:
  - `Validate` returns deterministic fail reasons.
  - `Reserve` acquires locks and reservation records.
  - `Commit` performs durable state write + outbox append atomically.
  - `Replicate` only consumes committed outbox events.
- Replication contract:
  - Snapshot message for full sync.
  - Delta message for incremental updates.
  - Monotonic sequence per zone/chunk.

## Sprint 0: Bootstrap Contracts

## Tasks

1) Define ID and correlation strategy
- Files:
  - `src/shared/Types/Ids.cs`
- Deliver:
  - Stable ID types for chunks, entities, actions, items.
  - Correlation ID and idempotency key shape.

2) Define protocol envelopes and versioning
- Files:
  - `src/shared/Protocol/Messages.cs`
- Deliver:
  - Message envelope with `protocol_version`, `sequence`, `correlation_id`.
  - Backward-compatible versioning rules.

3) Define persistence contract stubs
- Files:
  - `src/server/Persistence/Db.cs`
- Deliver:
  - Transaction wrapper conventions.
  - Outbox table/repository interface stub.

## Acceptance Criteria

- ID and message schemas are documented and used by terrain/action modules.
- Versioning and idempotency fields compile in shared protocol definitions.

## Sprint 1: Terrain Core + Replication (Corner-Height)

## Tasks

1) Define terrain primitives
- Files:
  - `src/shared/Types/Enums.cs`
  - `src/server/Terrain/TerrainChunk.cs`
- Deliver:
  - Corner grid model (`NW/NE/SW/SE` reference semantics).
  - Tile material/flags separated from corner heights.

2) Implement terrain math and validation
- Files:
  - `src/shared/Math/TerrainMath.cs`
- Deliver:
  - Slope computation from corner deltas.
  - Legal mutation checks (`dig/drop/flatten/level` boundaries).

3) Implement chunk locking + atomic border/shared-corner updates
- Files:
  - `src/server/Terrain/TerrainService.cs`
- Deliver:
  - Chunk ownership lock API.
  - Atomic mutation path for 1/2/4 chunk corner mutations.

4) Persist chunk state
- Files:
  - `src/server/Terrain/TerrainPersistence.cs`
  - `src/server/Persistence/Repositories/TerrainRepository.cs`
- Deliver:
  - Snapshot format with corner arrays + tile metadata.
  - Mutation append log per chunk.

5) Add terrain replication path
- Files:
  - `src/shared/Protocol/Messages.cs`
  - `src/server/Net/ReplicationService.cs`
  - `src/client/Scripts/TerrainView.cs`
- Deliver:
  - Terrain delta message type.
  - Client apply path for terrain corner/tile deltas.

## Acceptance Criteria

- Corner mutation survives server restart.
- Shared border/corner updates remain consistent after crash injection.
- Remote client observes terrain updates with `p95 < 250ms` at 50 test clients.
- Client reconnect snapshot matches authoritative terrain state.
- `0` border-corner divergence in 1,000 randomized cross-chunk tests.

## Sprint 2A: Action Pipeline Core (Server)

## Tasks

1) Create action contract and runner
- Files:
  - `src/server/Actions/ActionDefinition.cs`
  - `src/server/Actions/ActionRunner.cs`
  - `src/server/Actions/ActionContext.cs`
- Deliver:
  - Phases: `Validate -> Reserve -> Execute -> Resolve -> Commit -> Replicate`.

2) Add target locks and idempotency
- Files:
  - `src/server/Actions/ActionLocks.cs`
  - `src/server/Persistence/Repositories/ActionHistoryRepository.cs`
- Deliver:
  - Lock by entity/chunk/container ids.
  - Idempotency key enforcement and replay-safe response.

3) Add transactional outbox and sequence model
- Files:
  - `src/server/Actions/ActionOutbox.cs`
  - `src/server/Persistence/Repositories/OutboxRepository.cs`
- Deliver:
  - Commit writes gameplay state + outbox event in one DB transaction.
  - Monotonic per-zone/per-chunk event sequence.

## Acceptance Criteria

- Interrupted action never commits side effects.
- Duplicate client retry does not double-apply action (`0/10,000` duplicate applies).
- Crash after commit-before-send still delivers the committed event exactly once on restart.

## Sprint 2B: Action Replication + Client UI

## Tasks

1) Hook replication consumer
- Files:
  - `src/server/Net/ReplicationService.cs`
- Deliver:
  - Outbox drain and client fanout.

2) Hook action UI feed
- Files:
  - `src/client/Scripts/ActionUI.cs`
- Deliver:
  - Action start/progress/complete/cancel display from server events.

## Acceptance Criteria

- Action events arrive in sequence order for each actor stream.
- `p95` action event delivery latency `< 250ms` in local load test.

## Sprint 3: Trees Vertical Slice

## Tasks

1) Tree entity and state machine
- Files:
  - `src/server/Entities/TreeInstance.cs`
  - `src/server/Persistence/Repositories/TreeRepository.cs`
- Deliver:
  - States: `standing`, `felled`, `stump`, `removed`.

2) Implement tree actions
- Files:
  - `src/server/Actions/Implementations/ChopTreeAction.cs`
  - `src/server/Actions/Implementations/ProcessFelledTreeAction.cs`
  - `src/server/Actions/Implementations/RemoveStumpAction.cs`
- Deliver:
  - Timed chop with fall orientation.
  - Processing consumes remaining wood units.

3) Client world representation
- Files:
  - `src/client/Scripts/EntityViewTree.cs`
- Deliver:
  - Distinct visual states for standing/felled/stump.

## Acceptance Criteria

- Felling creates physical felled entity, not instant inventory loot.
- Processing depletes wood units and transitions to stump.
- Full tree lifecycle survives restart in `100/100` restart tests.

## Sprint 4: Beam-by-Beam Construction Slice

## Tasks

1) Build part schema and persistence
- Files:
  - `src/server/Entities/BuildPart.cs`
  - `src/server/Persistence/Repositories/BuildPartRepository.cs`
- Deliver:
  - States: `planned`, `framed`, `partial`, `complete`, `damaged`, `ruined`.

2) Implement construction actions for one wall type
- Files:
  - `src/server/Actions/Implementations/CreatePlanAction.cs`
  - `src/server/Actions/Implementations/DeliverMaterialAction.cs`
  - `src/server/Actions/Implementations/CompletePartAction.cs`
- Deliver:
  - BOM-driven material delivery and completion gate.

3) Client staged visuals
- Files:
  - `src/client/Scripts/EntityViewBuildPart.cs`
- Deliver:
  - Planned ghost, framed, partial, complete visuals.

## Acceptance Criteria

- Two players can contribute materials without duplication (`0/10,000` concurrent delivery dupes).
- Completion blocked until BOM is satisfied.
- Partial state survives restart and remains interactable.

## Sprint 5: Hardening and Test Gate

## Tasks

1) Expand regression suites
- Files:
  - `tests/Integration/TreeLifecycleTests.cs`
  - `tests/Integration/ConstructionFlowTests.cs`
  - `tests/Integration/TerrainPersistenceTests.cs`
  - `tests/Integration/TerrainReplicationTests.cs`
  - `tests/Integration/ActionIdempotencyTests.cs`
- Deliver:
  - Concurrency and restart-focused tests.

2) Add operational telemetry
- Files:
  - `src/server/World/WorldServer.cs`
  - `src/server/Net/ReplicationService.cs`
- Deliver:
  - Action failure reasons, lock contention, persistence latencies, outbox lag.

## Acceptance Criteria

- `0` dupes in stress scenarios.
- `0` terrain corruption from concurrent edge edits.
- Reboot/recovery restores consistent world state in `1,000` crash-injection runs.

## Test Gates Per Sprint

- Sprint 1 gate:
  - Terrain property tests and crash-recovery tests must pass.
- Sprint 2A gate:
  - Idempotency and lock-contention tests must pass before 2B starts.
- Sprint 3 gate:
  - Tree lifecycle restart tests must pass.
- Sprint 4 gate:
  - Construction restart + concurrency tests must pass.

## Database Starter Schema (Minimum)

- `terrain_chunks`
- `terrain_mutation_events`
- `terrain_mutation_chunks`
- `tree_instances`
- `build_parts`
- `build_part_requirements`
- `build_part_deliveries`
- `action_history`
- `action_outbox`
- `inventory_items`

## First Build Sequence (Practical)

1) Implement `TerrainChunk`, `TerrainMath`, and terrain persistence.
2) Implement terrain delta replication contract and client apply path.
3) Implement generic action runner core with outbox write on commit.
4) Implement `TreeInstance` + repository.
5) Implement `ChopTreeAction` using action runner.
6) Implement `DeliverMaterialAction` for one wall segment.
7) Add restart/crash tests before adding more gameplay breadth.

## Out of Scope for This Slice

- Full economy/market.
- Full cave system depth.
- Advanced AI ecology.
- Multi-node zone handoff.

Those come after this slice is stable and exploit-resistant.
