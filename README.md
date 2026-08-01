# Ancient History

Ancient History is an early-stage persistent sandbox MMO set around 0 AD, inspired by Wurm-style terrain modification, physical resource processing, gradual construction, and player-driven settlement life.

## Project status

The repository is currently building the first authoritative terrain vertical slice.

## Technical direction

- Godot 4.x .NET client.
- Standalone .NET authoritative world server.
- WebSocket transport initially.
- Versioned shared protocol contracts with JSON encoding initially.
- PostgreSQL as the durable dependency.
- One logical world with internal chunk and zone partitioning.
- Single-writer zone execution wherever practical.
- Corner-height terrain with one canonical owner per world corner.
- Transactional outbox, monotonic stream sequences, and idempotent commands.

## First playable milestone

1. Two Godot clients connect to one world server.
2. Both receive the same authoritative terrain chunk.
3. One client submits a legal corner-height mutation.
4. PostgreSQL commits the terrain change and replication event atomically.
5. Both clients apply the ordered delta.
6. Server restart restores identical terrain state.
7. Reconnect converges through replay or a fresh snapshot.

## Documentation

- [`docs/architecture.md`](docs/architecture.md)
- [`docs/networking.md`](docs/networking.md)
- [`docs/data-model.md`](docs/data-model.md)
- [`docs/simulation.md`](docs/simulation.md)
- [`docs/roadmap.md`](docs/roadmap.md)
- [`docs/implementation-plan.md`](docs/implementation-plan.md)
- [`docs/adr/`](docs/adr/)
- [`docs/development/`](docs/development/)
- [`AGENTS.md`](AGENTS.md)

## Development process

Use the lightest process that safely fits the change.

The implementation agent owns all command-line validation, including Bash, .NET, PowerShell and Windows executables invoked from WSL, headless Godot, server, database, networking, persistence, and recovery checks.

Human involvement is limited to opening Godot and checking affected visible or interactive behavior. No human evidence is required. Merging an editor-affecting pull request means the merger performed and accepted that editor check.

Reserve exhaustive specification and adversarial testing for material persistence, concurrency, recovery, protocol, security, authority, economy, and architecture risks.

See [`docs/development/process-principles.md`](docs/development/process-principles.md) and [`docs/development/workflow.md`](docs/development/workflow.md).