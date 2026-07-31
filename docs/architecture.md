# Architecture Overview

## Purpose

This document defines the target architecture for the first playable Wurm-style vertical slice and the constraints that preserve a path to a large, persistent, single-world MMO.

The immediate goal is not distributed scale. The immediate goal is to prove one authoritative, persistent gameplay loop end to end with strong recovery and exploit-resistance properties.

## Architecture Decisions for the Initial Slice

- Client runtime: Godot 4.x .NET.
- Authoritative world runtime: standalone .NET service with no Godot dependency.
- Initial transport: WebSocket with JSON messages behind a versioned protocol contract.
- Canonical persistence: PostgreSQL.
- World model: one logical world partitioned into chunks and zones.
- Execution model: single-writer zone executors wherever practical.
- Terrain authority: canonical corner-height ownership with rebuildable neighbor caches.
- Delivery model: transactional outbox with at-least-once transport and idempotent application.

These decisions are intentionally conservative. They minimize moving parts while preserving clean boundaries for later scaling.

## System Context

The system is divided into four primary areas:

1. Godot client
- Rendering, terrain meshes, animation, audio, UI, and input.
- Local presentation prediction and interpolation.
- Sends player intent, never authoritative state.
- Applies server snapshots and ordered deltas.

2. Authoritative world server
- Owns simulation time, player actions, terrain, entities, inventories, permissions, and outcomes.
- Validates every mutating command.
- Runs zone executors and authoritative ticks.
- Commits durable state before replication.

3. PostgreSQL persistence
- Stores canonical transactional state.
- Stores idempotency records, action history, mutation logs, snapshots, and transactional outbox records.
- Provides point-in-time recovery and migration support.

4. Operational services
- Health checks, metrics, structured logs, administration, and test tooling.
- Authentication and external platform integrations may be added after the first slice.

## Initial Deployment Topology

The first playable deployment consists of:

- One Godot client application.
- One authoritative .NET world-server process.
- One PostgreSQL instance.

Multiple clients connect to the same world-server process. The server internally partitions the map into zones, but all zones initially run in one process.

This is still a single-shard architecture because players share one identity namespace, one world, and one economy. Single shard does not require one machine, one process, or one thread indefinitely.

## World Execution Model

### Zones

- Chunks are grouped into zones.
- A zone executor is the exclusive writer for zone-local simulation state.
- Network handlers validate message shape and enqueue commands; they do not mutate gameplay state directly.
- Commands are processed in deterministic queue order within an authoritative tick.
- Cross-zone operations acquire zone authority in stable ascending order or are coordinated through an explicit cross-zone transaction path.

### Tick Model

Start with a configurable authoritative simulation rate in the 5-10 Hz range for heavy world systems.

Not every subsystem must update every tick:

- Player movement and nearby interactions may update frequently.
- Timed actions may use scheduled deadlines and coarse progress events.
- Ecology, decay, farming, and distant simulation may update at lower frequencies.
- Dormant zones may be event-driven or catch up from elapsed time.

### Commit and Replicate Flow

The authoritative mutation path is:

1. Receive and decode command.
2. Authenticate session and validate protocol version.
3. Resolve actor and target authority.
4. Check idempotency record.
5. Validate permissions, range, tools, revisions, and invariants.
6. Reserve required zone, chunk, entity, or container resources.
7. Commit domain changes and an outbox record in one PostgreSQL transaction.
8. Apply the committed result to in-memory authoritative state.
9. Publish ordered replication events from the outbox.
10. Return or replay the stored result for duplicate requests.

No client-visible success is acknowledged before durable commit.

## Terrain Architecture

### Source of Truth

- Surface elevation is stored as integer corner heights.
- Tile slope, normals, and mesh data are derived.
- Tile material, flags, paving, ownership, and other metadata are stored separately.
- Underground terrain is a separate layer and is deferred beyond the first vertical slice.

### Partitioning and Border Ownership

- Initial terrain chunk size: 64 x 64 tiles.
- A chunk contains a 65 x 65 logical corner grid.
- Every world-space corner has exactly one canonical owner chunk.
- Neighbor chunks may keep borrowed border or halo values for efficient reads and rendering.
- Borrowed values are caches, not competing persistent authority.
- Border mutations are routed to canonical owners and committed atomically across all affected owners.

The precise owner rule is versioned and covered by property-based tests.

## Client Responsibilities

The client may:

- Predict visual movement and progress presentation.
- Interpolate remote entities.
- Cache terrain and entity snapshots.
- Submit commands with expected revisions and idempotency keys.
- Detect sequence gaps and request resynchronization.

The client may not decide:

- Inventory ownership or quantities.
- Action completion.
- Skill or quality outcomes.
- Terrain legality.
- Position authority.
- Permission results.
- Economic transactions.

## Failure and Recovery Model

### Process Failure

- PostgreSQL commit is the durable boundary.
- On restart, the world server restores snapshots and replays committed mutation tails.
- Unpublished outbox records are republished.
- Clients reconnect with the last applied sequence for each subscribed stream.

### Data Recovery

- PostgreSQL point-in-time recovery is the initial recovery foundation.
- Chunk snapshots reduce replay time but do not replace mutation history.
- Snapshot payloads are checksummed and schema-versioned.
- Restore procedures must be automated and tested before production operation.

### Consistency Target

The initial architecture prioritizes correctness over availability:

- A zone may temporarily reject or delay commands during recovery.
- The server must not accept mutations when authority or durable state is uncertain.
- Duplicate delivery is tolerated; duplicate observable side effects are not.

## Security Baseline

- Treat all client data as untrusted.
- Use short-lived authenticated sessions.
- Rate-limit commands by session, actor, and command class.
- Validate range, movement, tools, permissions, cooldowns, revisions, and resource ownership server-side.
- Record privileged and economy-affecting actions in append-only audit history.
- Never deserialize arbitrary runtime types from client-provided payloads.

## Technology Deferred Until Proven Necessary

The following are explicitly deferred from the first vertical slice:

- Redis distributed locks or caches.
- Kafka, NATS JetStream, or another external event bus.
- Object storage for world snapshots.
- Kubernetes and multi-region deployment.
- Multi-node zone handoff.
- Seamless failover between authoritative world nodes.
- Custom UDP reliability protocol.

Interfaces may preserve future extension points, but no deferred dependency should be introduced without a measured requirement and an ADR.

## Initial Engineering Budgets

The implementation plan must establish and test explicit budgets for:

- Authoritative tick duration.
- Command queue depth and wait time.
- Terrain mutation latency.
- Database transactions per gameplay action.
- Outbox publication lag.
- Snapshot size and restore duration.
- Replication bytes per client per second.
- Maximum visible entities per client.
- Active players per zone.

Initial numerical targets belong in the feature specifications and are revised through profiling.

## Architecture Evolution

Scale through measured decomposition:

1. One process, multiple internal zone executors.
2. Move expensive or independent services out of process only when profiling justifies it.
3. Add multiple world nodes while retaining one logical world.
4. Introduce zone transfer and handoff after single-node correctness and recovery are proven.
5. Add specialized caches or event infrastructure only for demonstrated bottlenecks.

All irreversible or high-cost changes require an Architecture Decision Record in `docs/adr/`.
