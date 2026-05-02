# Architecture Overview

## System Context

The game is split into:
- Godot client(s): rendering, local prediction UX, UI, input.
- Authoritative simulation server cluster: gameplay truth.
- Persistence layer: relational + event log + blob stores.
- Platform services: auth, chat, metrics, admin tools.

## Suggested High-Level Stack

- Engine/client/server runtime: Godot 4.x (C# preferred for server-heavy logic consistency).
- Simulation servers: headless Godot instances or companion .NET service for heavy world logic.
- Primary DB: PostgreSQL.
- Cache/coordination: Redis.
- Durable event stream (optional at first): NATS JetStream or Kafka.
- Object storage: S3-compatible for snapshots, backups, logs.

## Logical Components

1) Gateway
- Session bootstrap, TLS termination, auth token verification.
- Routes player to world node / shard.

2) World Node
- Runs authoritative simulation for one world region group.
- Owns terrain mutation, entities, action processing.
- Produces snapshots/deltas to clients.

3) Action Service (can be in-world initially)
- Normalized action pipeline (start, ticks, completion, interruption).
- Skill checks, timers, stamina/failure rolls, item quality outcomes.

4) Economy & Persistence Service
- Inventory and container transactions.
- Trade contracts, mail, market orders.
- Write-through persistence and audit logs.

5) AI/Ecology Service (in-process at first)
- Creature brains, spawn balancing, migration.
- Resource regeneration and ecological pressure.

## Terrain Representation (Mandatory)

- Use a corner-height grid for surface terrain:
  - Each tile has four corners (`NW`, `NE`, `SW`, `SE`).
  - Elevation is stored per corner, not per tile center.
  - Tile slope is derived from corner deltas.
- Store tile metadata separately:
  - Surface type/material, packed/packed-state flags, paving, ownership/deed overlays.
- Keep underground/cave layer as a distinct corner-height field with structural metadata.
- Terraforming operations mutate one or more corners and then recompute affected tiles.

## World Partitioning

- World is partitioned into fixed-size chunks (example: 64m x 64m tile chunks).
- Chunks grouped into zones for node ownership.
- Zone ownership can move between nodes for scaling.
- Player handoff process:
  - Prewarm neighbor zone subscriptions.
  - Soft handoff with overlapping authority window.
  - Final authority transfer with monotonic tick fence.

## Authority and Timing

- Server tick: start at 5-10 Hz for heavy MMO simulation.
- Client render: uncapped/target 60+ FPS.
- Network snapshots: 5-20 Hz depending on entity class and distance.
- Critical actions are server-timed; client may show predicted progress bars but server decides completion.

## Failure Domains

- Zone/node crash should not corrupt global world.
- Regular incremental snapshots + append-only mutation logs.
- On restart, rebuild zone from snapshot + log tail.
- Chunk snapshots must include corner-height arrays + tile metadata + cave data.

## Security Baseline

- Zero trust of client state for inventory, position, timers, and action outcomes.
- Rate-limit all player commands.
- Signed session tokens with short expiration and refresh flows.
- Audit logs for privileged actions (GM/admin tools).
