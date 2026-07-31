# AGENTS.md

This document defines agent roles, ownership boundaries, and execution rules for building the Ancient History persistent sandbox MMO.

## Project Goal

Build a long-lifespan, Wurm-style medieval sandbox with:

- Slow progression, deep crafting, and terrain modification.
- Physical, persistent resource and construction workflows.
- A player-driven economy and settlement system.
- One logical world with authoritative server simulation.
- Strong exploit resistance through server ownership of truth.

## Current Delivery Goal

The current goal is the first authoritative terrain vertical slice:

1. Two Godot clients connect to one standalone .NET server.
2. A legal corner-height mutation commits in PostgreSQL.
3. Both clients receive the ordered delta.
4. Server restart recovers identical terrain state.
5. Reconnect converges through replay or snapshot.

Work that does not directly support this proof is deferred unless it fixes a blocking architectural risk.

## Core Principles

- Server authoritative: clients submit intent and render results.
- Persistence first: successful mutations have a durable commit point.
- Vertical slices before breadth or distributed scale.
- Deterministic rules where practical; committed outcomes are replay-safe.
- Data-driven definitions for recipes, skills, items, actions, and construction requirements.
- Single-writer zone execution wherever practical.
- Transactional outbox for commit-to-replication reliability.
- Idempotent mutating commands.
- Corner-height terrain with one canonical owner per world corner.
- Derived data is rebuildable from canonical state.

## Locked Initial Architecture

- Client: Godot 4.x .NET.
- Server: standalone .NET authoritative world service.
- Initial transport: WebSocket.
- Initial encoding: JSON behind versioned shared contracts.
- Durable storage: PostgreSQL.
- Initial deployment: one world-server process with internal zones.

Changes require an Architecture Decision Record in `docs/adr/`.

## Agent Topology

### 1. Product Architect Agent

Owns:

- Architecture and system boundaries.
- Milestone sequencing and acceptance gates.
- Non-functional requirements and engineering budgets.
- Architecture Decision Records.

Must prevent deferred scale or content work from bypassing the current milestone gate.

### 2. Gameplay Systems Agent

Owns:

- Actions, skills, quality, stamina, tools, crafting, decay, and construction rules.
- Persistent state-machine definitions.
- Data-driven tunables.

Constraints:

- Uses the shared authoritative action pipeline.
- Does not mutate persistence or replication directly.
- Defines interruption, retry, and restart behavior for every action.

### 3. World Simulation Agent

Owns:

- Terrain representation and mutation rules.
- Zone execution behavior.
- Persistent world entities.
- Later ecology, creatures, weather, and dormant-zone simulation.

Constraints:

- One canonical owner per terrain corner.
- Changes are zone-local when possible.
- Multi-owner changes are atomic.
- Derived values remain rebuildable.

### 4. Networking Agent

Owns:

- Protocol schemas and compatibility.
- Session bootstrap and reconnect.
- Snapshot and delta replication.
- Stream sequence handling and interest management.

Constraints:

- No gameplay rules in transport handlers.
- No canonical protocol dependency on Godot node paths or RPC annotations.
- Gaps, duplicates, and stale messages have explicit behavior.

### 5. Persistence and Economy Agent

Owns:

- PostgreSQL schema and migrations.
- Transactions, revisions, idempotency, audit history, and outbox.
- Inventory and economy integrity.
- Backup, restore, and recovery verification.

Constraints:

- Domain state and outbox append commit atomically.
- Economy operations are ACID or use an explicitly documented compensating model.
- In-memory repositories are test doubles, not production substitutes.

### 6. Infrastructure and DevOps Agent

Owns:

- Reproducible local bootstrap.
- CI, deployment, configuration, observability, and backups.
- Load, soak, and failure-injection environments.

Constraints:

- No developer-specific paths in committed tooling.
- One-command setup for required local dependencies.
- Distributed infrastructure is introduced only after a measured need and ADR.

### 7. QA and Verification Agent

Owns:

- Unit, property, integration, failure-injection, and end-to-end test strategy.
- Terrain seam and recovery suites.
- Duplication and conservation tests.
- Milestone release gates.

Constraints:

- Critical invariant failures block progression.
- Acceptance tests are defined before feature completion.
- Test counts and latency claims identify the environment in which they were measured.

## Execution Workflow

1. Product Architect confirms the next milestone and ADRs.
2. QA defines acceptance and failure-injection coverage.
3. Shared contracts are agreed before client, server, and persistence implementation diverge.
4. Persistence defines transaction and recovery semantics.
5. Simulation implements pure rules and property tests.
6. Networking integrates snapshots, deltas, and reconnect.
7. Client implements presentation and input.
8. CI and observability validate the complete path.
9. The next milestone begins only after exit criteria pass.

## Change Requirements

Every subsystem change documents:

- API or protocol impact.
- Persistence and migration impact.
- Retry, disconnect, and restart behavior.
- Telemetry and failure diagnostics.
- Tests added or updated.
- ADR impact for high-cost or irreversible decisions.

## Ownership Map

- `docs/architecture.md`: Product Architect.
- `docs/adr/`: Product Architect plus affected owner.
- `docs/simulation.md`: Gameplay and World Simulation.
- `docs/networking.md`: Networking.
- `docs/data-model.md`: Persistence and Economy.
- `docs/roadmap.md`: Product Architect, QA, and Infrastructure.
- `docs/implementation-plan.md`: Product Architect and all implementation owners.
- `docs/feature-*.md`: Named feature owner plus QA.

## Definition of Done

A milestone or feature is complete only when:

- Functional acceptance criteria pass.
- Server-authoritative invariants pass.
- Required persistence migrations and recovery paths pass.
- Duplicate, concurrency, disconnect, and restart tests pass.
- Metrics and structured diagnostics exist for new failure modes.
- Documentation matches implemented behavior.
- No deferred dependency was introduced without measured justification and an ADR.
