# AGENTS.md

This document defines the durable project boundaries and working rules for agents building the Ancient History persistent sandbox MMO.

## Project Goal

Build a long-lifespan, Wurm-style ancient-world sandbox set around 0 AD with slow progression, deep crafting, terrain modification, persistent physical workflows, a player-driven economy, and one authoritative logical world.

## Current Delivery Goal

Prove the first authoritative terrain vertical slice:

1. Two Godot clients connect to one standalone .NET server.
2. A legal corner-height mutation commits in PostgreSQL.
3. Both clients receive the ordered delta.
4. Server restart recovers identical terrain state.
5. Reconnect converges through replay or snapshot.

Work that does not directly support this proof is deferred unless it removes a blocking architectural risk.

## Core Principles

- Server authoritative: clients submit intent and render results.
- Persistence first: successful mutations have a durable commit point.
- Vertical slices before breadth or distributed scale.
- Deterministic rules where practical; committed outcomes are replay-safe.
- Data-driven definitions for recipes, skills, items, actions, and construction.
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

High-cost or irreversible changes require an Architecture Decision Record in `docs/adr/`. Ordinary implementation choices do not.

## Ownership

- Product architecture: boundaries, milestone sequence, ADRs, engineering budgets.
- Gameplay systems: actions, skills, quality, stamina, tools, crafting, decay, and construction rules.
- World simulation: terrain, zone execution, persistent world entities, ecology, creatures, and weather.
- Networking: protocol schemas, sessions, reconnect, snapshots, deltas, sequencing, and interest management.
- Persistence and economy: schema, migrations, transactions, revisions, idempotency, outbox, inventory, and economy integrity.
- Infrastructure: reproducible local bootstrap, CI, deployment, configuration, observability, and backups.
- QA and verification: tests and milestone gates proportional to risk.

Agents may work across ownership areas when the objective requires it. Ownership identifies architectural responsibility, not mandatory handoffs or serial agent ceremonies.

## Non-Negotiable Boundaries

- Gameplay and persistent shared state remain server authoritative.
- Transport handlers do not own gameplay rules.
- Domain state and required outbox records commit atomically.
- In-memory repositories are test doubles, not production substitutes.
- One canonical owner exists for each terrain corner.
- Multi-owner state changes are atomic.
- Developer-specific paths and committed binaries are prohibited.
- Distributed infrastructure requires measured need and an ADR.

## Development Process

Follow `docs/development/process-principles.md` and `docs/development/workflow.md`.

Process is risk-based:

- Low-risk changes use concise descriptions and focused validation.
- Medium-risk features use observable acceptance criteria and tests for realistic failures.
- High-risk persistence, concurrency, recovery, protocol, security, economy, authority, and architecture work receives explicit invariants and deeper review.

Observe real tools and interfaces before specifying exact output or behavior. Block only on material failures in supported workflows or required invariants. Record nonessential hardening as follow-up work.

## Agent Execution Rules

Agents must:

1. Read the relevant issue and governing documents.
2. Preserve the architecture and authority boundaries above.
3. Inspect real supported tools or interfaces before encoding exact assumptions when practical.
4. Keep work focused on the observable objective.
5. Add tests proportional to the affected behavior and risk.
6. Report validation honestly and concisely.
7. Stop for unresolved architecture, incompatible dependencies, public-contract changes, or significant scope expansion.

Agents may resolve minor unspecified details using existing repository conventions. They do not need to stop for every ambiguity, predict every malformed input, or produce exhaustive evidence for low-risk work.

## Change Documentation

Document only applicable impacts:

- Public API or protocol changes.
- Persistence, migration, retry, restart, or recovery behavior.
- Authority or security implications.
- New failure diagnostics when they materially help operation.
- Tests and human validation for affected behavior.
- ADR impact for high-cost or irreversible decisions.

Writing “not applicable” across a universal checklist is not required.

## Definition of Done

A change is complete when:

- Its observable objective works.
- Required tests for the affected behavior and risk pass.
- No material correctness, durability, compatibility, authority, security, or architecture blocker remains.
- Required human validation passes.
- Documentation that users or future implementers depend on matches behavior.

Concurrency, duplicate, recovery, migration, telemetry, screenshot, and exact-head evidence are required only when applicable to the change. A later commit invalidates prior human evidence only when it can affect the tested behavior.
