# ADR-0001: Standalone .NET Authoritative Server

- Status: Accepted
- Date: 2026-07-31

## Context

The project uses Godot for the client and requires a persistent authoritative simulation for terrain, actions, inventories, entities, recovery, and later economy systems.

The server could run as a headless Godot application or as a standalone .NET service. Most authoritative systems do not require rendering, scene-tree ownership, or engine physics.

## Decision

Use a standalone .NET service as the initial authoritative world server.

Godot remains the client runtime. Shared protocol and value contracts live in a Godot-independent .NET project referenced by both client and server.

## Consequences

### Positive

- Authoritative simulation can run and test without launching Godot.
- Server hosting, dependency injection, PostgreSQL access, health checks, and background workers use standard .NET patterns.
- Protocol contracts are not coupled to Godot node paths or RPC annotations.
- Load, property, integration, and failure-injection tests are easier to automate.
- Server deployment does not include rendering assets or engine runtime unless later required.

### Negative

- Client and server need explicit transport and protocol adapters.
- Godot-specific physics or navigation cannot be reused directly on the server.
- Shared code must avoid engine-specific types.

## Alternatives Considered

### Headless Godot Server

Valid when authoritative gameplay depends heavily on Godot physics, navigation, or scene-tree behavior. Rejected for the initial slice because terrain, actions, persistence, inventories, and construction are primarily data and transaction systems.

### Hybrid Server

A standalone .NET world server with specialized headless Godot workers may be considered later if a measured subsystem requires engine simulation.

## Revisit Conditions

Revisit only if profiling or feature requirements demonstrate that authoritative engine physics or navigation provides more value than the additional deployment and coupling cost.
