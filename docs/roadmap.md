# Roadmap and Milestones

## Roadmap Philosophy

Milestones are evidence gates, not calendar promises. A milestone is complete only when its functional, persistence, correctness, and operational criteria pass in automation or a documented test environment.

The project should not begin distributed scaling work until one authoritative, persistent gameplay slice is reliable under retries, concurrency, disconnects, and process restarts.

## Current Status

The repository is in Sprint 0 contract scaffolding.

Implemented in prototype form:

- Shared identifier wrappers.
- Versioned message envelope.
- In-memory idempotency and outbox interfaces.
- Basic lock ordering comparator.
- Minimal contract test executable.

Not yet implemented:

- Runnable authoritative world server.
- Playable Godot client scene.
- PostgreSQL persistence and migrations.
- Real idempotency conflict semantics.
- Transactional outbox and sequence allocation.
- Terrain model, mutation, recovery, or replication.
- End-to-end client/server connection.

## Milestone 0: Reproducible Foundation

### Objective

Make the repository buildable, testable, and runnable by a new contributor without machine-specific paths.

### Deliverables

- Pin the supported Godot .NET version.
- Add a portable Godot launcher or documented `GODOT_BIN` convention.
- Add `global.json` and shared .NET build settings.
- Add an executable standalone world-server project.
- Add a minimal Godot client project and startup scene.
- Add PostgreSQL development configuration and migrations.
- Add CI for build, tests, formatting, and headless Godot project validation.
- Add root documentation for bootstrap, test, server, and client commands.

### Exit Criteria

- Clean checkout builds in CI.
- One command starts PostgreSQL and the world server.
- Godot client opens and establishes a session with the server.
- Automated tests run without a developer-specific file path.
- No production-intent dependency is represented only by an in-memory stub without explicit test-only naming.

## Milestone 1: Sprint 0 Delivery Semantics

### Objective

Complete the contracts that every authoritative mutation depends on.

### Deliverables

- Validated typed identifiers and coordinate types.
- Explicit protocol major and minor compatibility behavior.
- Typed command, result, rejection, snapshot, and delta contracts.
- PostgreSQL-backed idempotency store.
- Transactional outbox.
- Per-stream sequence allocator.
- Outbox publisher and replay behavior.
- Single-writer zone command queue skeleton.
- Contract and failure-injection test suites.

### Exit Criteria

- Same key and same request hash returns the original result.
- Same key and different request hash always returns conflict.
- `0/10,000` duplicate observable side effects under concurrent retries.
- Crash after commit and before publish converges through outbox replay.
- Stream duplicate, reorder, and gap handling tests pass.
- Protocol compatibility tests pass for supported versions.

## Milestone 2: Authoritative Terrain Core

### Objective

Implement deterministic, persistent Wurm-style corner-height terrain without client rendering dependencies.

### Deliverables

- 64 x 64 tile terrain chunks.
- Canonical world-corner ownership.
- Typed corner mutations.
- Integer slope and legality validation.
- Single- and multi-owner atomic mutation path.
- Snapshot, mutation-tail replay, and checksums.
- Property-based seam and recovery tests.
- Terrain-specific metrics.

### Exit Criteria

- `0` seam mismatches in 10,000 randomized border and shared-corner mutations.
- `0` duplicate applies under command retry tests.
- 100/100 crash-injection recovery runs produce identical canonical terrain state and revisions.
- Derived slope and mesh inputs are reproducible from canonical corners.
- Terrain mutation latency meets the feature-specification budget in the local test environment.

## Milestone 3: First Playable Network Slice

### Objective

Prove the complete path from Godot input to durable server commit and replicated client state.

### Deliverables

- Client session bootstrap.
- One subscribed terrain chunk.
- Terrain snapshot delivery.
- Basic Godot terrain mesh generation.
- Client corner selection and dig/drop command.
- Server validation, durable commit, and delta publication.
- Multi-client observation.
- Disconnect, reconnect, and resynchronization.

### Exit Criteria

- Two clients observe the same terrain mutation in sequence.
- Terrain survives a server restart.
- Reconnecting client converges to authoritative state.
- Deliberate duplicate, delayed, and reordered test messages do not corrupt client state.
- No client can submit an illegal slope, stale revision, or unauthorized mutation successfully.

This milestone is the first project definition of playable.

## Milestone 4: Generic Timed Action Pipeline

### Objective

Implement the reusable authoritative action lifecycle required by gathering, crafting, construction, and combat.

### Deliverables

- `Validate -> Reserve -> Execute -> Resolve -> Commit -> Replicate` pipeline.
- Authoritative action deadlines and progress state.
- Cancellation, interruption, range loss, and disconnect behavior.
- Resource reservation and release.
- Action history and idempotent results.
- Client action UI driven by server events.

### Exit Criteria

- Interrupted actions do not commit side effects.
- Restart during every action phase produces a valid deterministic outcome.
- Duplicate action commands do not duplicate outputs or resource consumption.
- Progress and completion displayed by clients match authoritative state.

## Milestone 5: Tree Lifecycle Slice

### Objective

Prove a persistent physical resource workflow rather than instant inventory loot.

### Deliverables

- Standing, felled, processed, stump, and removed tree states.
- Timed chopping and processing actions.
- Physical fall orientation and client representation.
- Durable wood-unit depletion.
- Restart and concurrent-interaction tests.

### Exit Criteria

- Felling creates a persistent felled entity.
- Processing consumes remaining wood units exactly once.
- Tree state survives 100/100 restart tests.
- Concurrent actions cannot duplicate wood or produce invalid state transitions.

## Milestone 6: Construction Slice

### Objective

Prove persistent multi-player contribution and staged world building.

### Deliverables

- One wall-part plan and bill of materials.
- Material delivery transactions.
- Planned, framed, partial, complete, damaged, and ruined states as needed by the initial wall.
- Staged client visuals.
- Concurrent contribution tests.

### Exit Criteria

- Two players can contribute without duplication in 10,000 concurrent delivery trials.
- Completion is impossible before requirements are satisfied.
- Partial construction survives restart and remains interactable.
- Permission and ownership checks are authoritative.

## Milestone 7: Integrated Survival and Crafting Loop

### Objective

Connect terrain, trees, actions, inventory, tools, and construction into a coherent first-hour gameplay loop.

### Candidate Scope

- Character inventory and containers.
- A small skill set.
- A limited item-definition catalog.
- Gather, process, craft, improve, and repair.
- Basic stamina and tool durability.
- One shelter or work-area objective.

### Exit Criteria

- A fresh character can gather resources, create a tool, alter terrain, fell and process a tree, and construct one complete wall segment.
- No item duplication or lost committed state in the stress suite.
- Server restart preserves the entire workflow state.

## Milestone 8: Settlement and Economy Foundation

Begins only after the integrated loop is stable.

Candidate scope:

- Deeds and permissions.
- Structure ownership and upkeep.
- Direct player trade.
- Market or order-board prototype.
- Economy audit and integrity tooling.

## Milestone 9: Scale and Operational Hardening

Begins only after profiling a stable single-node world.

Candidate scope:

- AOI optimization.
- Dormant-zone simulation.
- Load and soak clients.
- Zone transfer between world nodes.
- Failure-domain isolation.
- Operational dashboards, alerts, backups, and restore drills.

### Scale Gate

Do not introduce multi-node zone handoff until:

- Single-node zone execution is correct under load.
- Persistent recovery is automated.
- Replication and AOI budgets are measured.
- A demonstrated bottleneck cannot be solved economically within one process.

## Explicitly Deferred Backlog

- Full cave system.
- Advanced ecology and migration.
- Elaborate combat.
- Global market infrastructure.
- Guilds, mail, and social events.
- Localization and accessibility completion.
- Bot and fraud detection platform.
- Multi-region deployment.
- External event streaming infrastructure.

Deferred items may be researched, but they should not displace milestone exit criteria.
