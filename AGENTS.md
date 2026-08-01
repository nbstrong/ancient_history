# AGENTS.md

This document defines the durable project boundaries and working rules for agents building the Ancient History persistent sandbox MMO.

## Project goal

Build a long-lifespan, Wurm-style ancient-world sandbox set around 0 AD with slow progression, deep crafting, terrain modification, persistent physical workflows, a player-driven economy, and one authoritative logical world.

## Current delivery goal

Prove the first authoritative terrain vertical slice:

1. Two Godot clients connect to one standalone .NET server.
2. A legal corner-height mutation commits in PostgreSQL.
3. Both clients receive the ordered delta.
4. Server restart recovers identical terrain state.
5. Reconnect converges through replay or snapshot.

## Core principles

- Server authoritative: clients submit intent and render results.
- Persistence first: successful mutations have a durable commit point.
- Vertical slices before breadth or distributed scale.
- Deterministic rules where practical; committed outcomes are replay-safe.
- Single-writer zone execution wherever practical.
- Transactional outbox and idempotent mutating commands.
- One canonical owner per terrain corner.
- Derived data is rebuildable from canonical state.

## Locked initial architecture

- Client: Godot 4.x .NET.
- Server: standalone .NET authoritative world service.
- Initial transport: WebSocket.
- Initial encoding: JSON behind versioned shared contracts.
- Durable storage: PostgreSQL.
- Initial deployment: one world-server process with internal zones.

High-cost or irreversible changes require an ADR. Ordinary implementation choices do not.

## Non-negotiable boundaries

- Persistent shared state remains server authoritative.
- Transport handlers do not own gameplay rules.
- Domain state and required outbox records commit atomically.
- In-memory repositories are test doubles, not production substitutes.
- Multi-owner state changes are atomic.
- Developer-specific paths and committed binaries are prohibited.
- Distributed infrastructure requires measured need and an ADR.

## Agent ownership

Agents may work across project areas when the objective requires it. Ownership describes architectural responsibility, not mandatory handoffs.

The implementation agent owns all non-visual validation. This includes:

- .NET builds and tests.
- Bash scripts.
- Windows PowerShell scripts.
- Headless Godot import/build/runtime checks.
- Server, database, networking, persistence, recovery, and compatibility checks.

WSL is the normal agent environment. Agents may invoke `powershell.exe`, `pwsh`, Windows executables, and other host tools from WSL. Do not ask a human to copy and run terminal commands merely because a check uses Windows tooling.

## Human involvement

Humans are used only for the part automation cannot replace: opening Godot and checking affected visible or interactive behavior.

No human test report, screenshot, video, command transcript, environment record, tested SHA, or PR comment is required. Merging an editor-affecting pull request means the merger performed and accepted the editor check.

## Agent execution rules

Agents must:

1. Read relevant issues and governing documents.
2. Preserve architecture, authority, durability, and compatibility boundaries.
3. Inspect real tools before encoding exact assumptions.
4. Keep work focused on the observable objective.
5. Add tests proportional to affected behavior and risk.
6. Run every practical command-line check themselves, including PowerShell from WSL.
7. Report validation honestly and concisely.
8. Stop only for unresolved architecture, incompatible dependencies, public-contract changes, or significant scope expansion.

Agents may resolve minor unspecified details through repository conventions. They do not need to predict every unsupported edge case or produce exhaustive evidence.

## Definition of done

A change is complete when:

- Its observable objective works.
- Required automated checks pass.
- No material correctness, durability, compatibility, authority, security, or architecture blocker remains.
- The merger is satisfied with any required editor check.
- Documentation relied on by users or future implementers matches behavior.

Follow `docs/development/process-principles.md` and `docs/development/workflow.md`.