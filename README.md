# Ancient History

Ancient History is an early-stage persistent medieval sandbox MMO project inspired by Wurm-style terrain modification, physical resource processing, gradual construction, and player-driven settlement life.

## Project Status

The repository is currently in Sprint 0 contract scaffolding.

The documentation defines the intended architecture and implementation gates. The current source contains prototype shared contracts, in-memory outbox and idempotency implementations, and a minimal contract test executable. It does not yet contain a playable client/server vertical slice.

## Current Technical Direction

- Godot 4.x .NET client for rendering, UI, input, and presentation prediction.
- Standalone .NET authoritative world server.
- WebSocket transport for the initial vertical slice.
- Versioned shared protocol contracts with JSON encoding initially.
- PostgreSQL as the only required durable dependency.
- One logical world with internal chunk and zone partitioning.
- Single-writer zone execution wherever practical.
- Wurm-style corner-height terrain with one canonical owner per world corner.
- Transactional outbox, monotonic stream sequences, and idempotent commands.

## First Playable Milestone

The first playable proof is intentionally narrow:

1. Two Godot clients connect to one world server.
2. Both receive the same authoritative terrain chunk.
3. One client submits a legal corner-height mutation.
4. PostgreSQL commits the terrain change and replication event atomically.
5. Both clients apply the ordered delta.
6. Server restart restores identical terrain state.
7. Reconnect converges through delta replay or a fresh snapshot.

Trees, construction, crafting breadth, settlements, caves, advanced ecology, and distributed zone handoff follow this milestone.

## Documentation

- [`docs/architecture.md`](docs/architecture.md): system boundaries, deployment topology, authority, and evolution.
- [`docs/networking.md`](docs/networking.md): WebSocket-first protocol, streams, snapshots, deltas, and reconnect.
- [`docs/data-model.md`](docs/data-model.md): PostgreSQL schema direction, transactions, outbox, snapshots, and recovery.
- [`docs/simulation.md`](docs/simulation.md): authoritative scheduling, action pipeline, terrain, and protected invariants.
- [`docs/roadmap.md`](docs/roadmap.md): milestone gates and current project status.
- [`docs/implementation-plan.md`](docs/implementation-plan.md): ordered pull-request execution plan.
- [`docs/feature-sprint-0-contracts.md`](docs/feature-sprint-0-contracts.md): delivery-semantics acceptance criteria.
- [`docs/feature-step-1-terrain-core.md`](docs/feature-step-1-terrain-core.md): authoritative terrain specification.
- [`docs/adr/`](docs/adr/): architecture decision records.
- [`AGENTS.md`](AGENTS.md): ownership and execution rules.

## Development Bootstrap

The reproducible bootstrap is part of the first implementation milestone and is not complete yet.

The target developer workflow is:

```text
./dev bootstrap
./dev test
./dev server
./dev client
```

Until those commands exist, follow the current project files directly and treat machine-specific launch scripts as temporary.

## Contribution Rule

Complete the current milestone's automated exit criteria before expanding scope. High-cost or irreversible technical changes require an Architecture Decision Record in `docs/adr/`.
