# AGENTS.md

This document defines agent roles, ownership boundaries, and execution workflow for building this Wurm-style MMO in Godot.

## Project Goal

Build a long-lifespan, persistent sandbox MMO inspired by Wurm-style gameplay:
- Slow progression with deep crafting and terraforming.
- Player-driven economy and settlement building.
- Large contiguous world with authoritative server simulation.
- Strong anti-cheat through server ownership of truth.

## Core Principles

- Server authoritative: clients are presentation and input devices.
- Deterministic simulation where practical for consistency and replay tooling.
- Data-driven systems: recipes, skills, item defs, actions in structured data.
- Vertical slices first, then scale and hardening.
- Persistence-first design: every gameplay system must serialize cleanly.
- Terrain model is corner-height based (Wurm-style), not per-tile scalar height.

## Agent Topology

### 1) Product Architect Agent

Owns:
- High-level architecture and system boundaries.
- Feature prioritization and milestone sequencing.
- Non-functional requirements: scale, resilience, operability.

Outputs:
- Architecture decisions (ADRs).
- Milestone acceptance criteria.
- Cross-team dependency maps.

### 2) Gameplay Systems Agent

Owns:
- Skills, actions, crafting, item quality, decay, stamina, hunger.
- Terraforming/mining/felling/farming mechanics.
- Combat/PvE baseline loops.

Constraints:
- Must use shared action pipeline and authoritative server APIs.
- Must expose all tunables as data.

### 3) World Simulation Agent

Owns:
- Terrain tiles/voxels strategy and server-side map representation.
- Resource regeneration, ecology, weather/time impacts.
- AI creatures, spawn systems, pathing abstractions.

Constraints:
- Changes must be chunk-local when possible.
- Mutation history must be persisted incrementally.

### 4) Networking Agent

Owns:
- Protocol schema, snapshot/delta replication, reliability model.
- Interest management, AOI, shard/zone transitions.
- Session/auth glue and anti-cheat input validation.

Constraints:
- No direct gameplay logic in transport layer.
- Backward-compatible protocol evolution policy.

### 5) Persistence & Economy Agent

Owns:
- Database schema for accounts, characters, items, deeds, markets.
- Transactional operations and idempotent server commands.
- Auction/trade systems and economy telemetry.

Constraints:
- Every player-visible state change is auditable.
- Inventory/economy operations must be ACID or compensating-transaction safe.

### 6) Infrastructure & DevOps Agent

Owns:
- Local/dev environments, CI/CD, deployment topology.
- Observability, alerting, backups, disaster recovery drills.
- Load/perf testing harnesses.

Constraints:
- Reproducible builds and one-command local bootstrap.
- Canary + rollback strategy required before major releases.

### 7) QA/Verification Agent

Owns:
- Test strategy across unit/integration/simulation/end-to-end.
- Regression suites for economy duplication and terrain mutation bugs.
- Balance sanity checks and soak tests.

Constraints:
- Blocks release when critical invariant tests fail.
- Maintains bug taxonomy and reproducibility templates.

## Execution Workflow

1) Product Architect defines vertical slice scope.
2) Gameplay + World + Networking agree on contracts first.
3) Persistence defines storage and transaction semantics.
4) Infra provisions environments and telemetry before scale tests.
5) QA defines acceptance tests before implementation freeze.
6) Release gated by test pass, perf budgets, and rollback validation.

## Coordination Rules

- Every subsystem change requires:
  - API contract update.
  - Persistence impact note.
  - Telemetry/event update.
- Avoid cross-agent file ownership conflicts by assigning modules explicitly.
- Use ADRs for irreversible decisions (terrain model, transport model, DB choices).

## Initial Ownership Map

- `docs/architecture.md`: Product Architect
- `docs/simulation.md`: Gameplay + World Simulation
- `docs/networking.md`: Networking
- `docs/data-model.md`: Persistence & Economy
- `docs/roadmap.md`: Product Architect + QA + Infra

## Definition of Done (Per Milestone)

- Functional: acceptance criteria met in playable build.
- Technical: server authoritative invariants pass.
- Operational: metrics, logs, and alerts live.
- Persistence: migration and rollback path tested.
- Quality: automated tests at target coverage threshold for touched modules.
