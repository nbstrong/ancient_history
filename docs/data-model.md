# Data Model and Persistence

## Purpose

This document defines the initial persistence architecture and integrity rules for the authoritative world server. PostgreSQL is the sole required durable dependency for the first playable vertical slice.

## Initial Storage Decision

Use PostgreSQL for:

- Canonical transactional gameplay state.
- Idempotency and command results.
- Action and mutation audit history.
- Terrain snapshots and mutation tails.
- Transactional outbox records.
- Stream sequence allocation.
- Schema migrations and point-in-time recovery.

Do not introduce Redis, an external event bus, or object storage until profiling or operational scale demonstrates a concrete requirement.

## Persistence Principles

- A successful player-visible mutation must have one durable commit point.
- Domain state and its outbox event commit in the same transaction.
- Retried commands must not produce duplicate observable side effects.
- Persisted payloads are schema-versioned.
- Derived data must be rebuildable from canonical state.
- Deleted entity identifiers are never reused.
- Every economy-affecting or world-mutating action is auditable.
- Recovery behavior is part of the feature definition, not a later operational concern.

## Identifier Rules

- Entity-like identities use validated typed identifiers.
- World coordinates use explicit coordinate value types, not formatted strings.
- Database columns use native UUID, integer, or structured coordinate fields where practical.
- Public wire formats are stable and versioned.
- Empty, malformed, or unrecognized identifiers are rejected at boundaries.

## Initial Schema Scope

### Sprint 0: Delivery Semantics

- `schema_migrations`
- `action_history`
- `action_outbox`
- `stream_sequences`

### Sprint 1: Terrain

- `terrain_chunks`
- `terrain_mutation_events`
- `terrain_mutation_chunks`

### Later Vertical-Slice Tables

- `characters`
- `sessions`
- `entity_instances`
- `tree_instances`
- `containers`
- `inventory_items`
- `build_parts`
- `build_part_requirements`
- `build_part_deliveries`

Accounts, skills, settlements, markets, creatures, and broader economy tables are deferred until their corresponding vertical slice.

## Idempotency Model

Mutating commands use uniqueness scope:

`(actor_id, idempotency_key)`

The first accepted command stores:

- Actor identifier.
- Idempotency key.
- Request hash.
- Command type.
- Status.
- Result or rejection payload.
- Created and completed timestamps.
- Optional expiration timestamp.

Required behavior:

- Same key and same hash returns the stored result.
- Same key and different hash returns a deterministic conflict.
- Concurrent first submissions resolve to one winner through a database uniqueness constraint.
- No side effect is applied outside the transaction that establishes the idempotency outcome.
- Initial retention is at least 24 hours and may be longer for economy-sensitive commands.

## Transactional Outbox

`action_outbox` contains:

- `outbox_id`
- `stream_id`
- `sequence`
- `event_type`
- `dedupe_key`
- `payload_schema_version`
- `payload_json`
- `created_at`
- `published_at`
- `publish_attempts`
- `last_publish_error`

Required constraints:

- Unique `(stream_id, sequence)`.
- Unique dedupe key within its defined event scope.
- Sequence allocation occurs in the same transaction as domain state and outbox append.
- Publisher retries are safe.
- Marking an event published is operational bookkeeping and does not alter domain state.

## Transaction Pattern

A mutating command follows this database pattern:

1. Begin transaction.
2. Establish or load the idempotency record.
3. Return stored result or conflict when already resolved.
4. Lock or conditionally update the required canonical rows.
5. Validate expected revisions and domain invariants.
6. Write domain state changes.
7. Append audit or mutation history.
8. Allocate stream sequence numbers.
9. Append outbox records.
10. Store the final command result.
11. Commit.

The in-memory world applies the committed result after the database commit. If the process fails after commit, recovery reconstructs the same result from durable state.

## Revision and Concurrency Model

Canonical mutable aggregates carry monotonic revisions.

Examples:

- Terrain chunk revision.
- Entity revision.
- Container revision.
- Build-part revision.

Commands include expected revisions when operating on client-cached state. A stale revision returns a typed conflict without partial effects.

Single-writer zone execution reduces in-process contention, but database constraints remain the final integrity boundary for cross-zone, inventory, and recovery scenarios.

## Terrain Persistence

### Canonical State

A terrain chunk snapshot stores:

- Chunk identifier and origin.
- Dimensions.
- Canonically owned corner elevations.
- Tile material and flags.
- Snapshot schema version.
- Snapshot revision watermark.
- Checksum.
- Updated timestamp.

Borrowed border or halo values are not competing authority and may be regenerated from owner chunks.

### Mutation History

A terrain mutation records:

- Mutation identifier.
- Actor and idempotency key.
- Mutation type.
- Creation and commit timestamps.
- Overall status.
- Per-owner-chunk pre- and post-revisions.
- Typed corner deltas.
- Payload schema version.

A multi-chunk mutation commits all affected owner-chunk deltas atomically.

### Snapshot and Replay

Recovery for each chunk is:

1. Load the newest valid snapshot.
2. Verify schema version and checksum.
3. Replay committed mutation records after the snapshot watermark in revision order.
4. Rebuild derived terrain data and borrowed halos.
5. Verify final revision continuity.

Snapshots are optimization checkpoints. Mutation history remains the audit and recovery tail.

## Serialization Rules

- Every persisted JSON or binary payload includes a schema version.
- Canonical numerical values use explicit units.
- Terrain elevations use integer decimeters.
- Date and time values use UTC.
- Floating-point derived data is not persisted as authority when deterministic integer inputs can regenerate it.
- Migrations must preserve old payload readability or include an explicit transformation step.

## Migrations

- Use ordered, immutable database migrations.
- CI applies migrations to an empty database and upgrades from the previous supported schema.
- Destructive migrations require backup and rollback procedures.
- Application startup verifies the schema version before accepting player commands.
- Developer bootstrap uses the same migrations as deployed environments.

## Recovery Strategy

Initial recovery capabilities:

- PostgreSQL point-in-time recovery.
- Automated database backup verification.
- Terrain snapshot plus mutation-tail replay.
- Outbox replay after process restart.
- Idempotent reprocessing of duplicate client commands.

Before any external alpha:

- Define recovery point and recovery time objectives.
- Automate a full restore into an isolated environment.
- Verify world invariants after restore.
- Document operator steps and rollback criteria.

## Integrity Checks

Automated checks include:

- Duplicate item ownership.
- Invalid or cyclic container graphs.
- Missing or discontinuous terrain revisions.
- Duplicate stream sequences.
- Outbox records without corresponding committed domain state.
- Committed commands without stored results.
- Borrowed terrain halo mismatches against canonical owner corners.
- Build requirements with negative or excessive delivered quantities.

Checks run in integration tests first and become scheduled operational jobs when persistent environments exist.

## Observability

Track at minimum:

- Transaction latency by command type.
- Lock wait and serialization failure rates.
- Idempotency replay and conflict counts.
- Outbox backlog, age, attempts, and failures.
- Snapshot save and load latency.
- Mutation replay count and duration.
- Revision-continuity and checksum failures.
- Database connection-pool pressure.

## Deferred Storage Components

Possible future components include:

- Redis for measured cache or coordination needs.
- S3-compatible object storage for large snapshots and backups.
- NATS JetStream or Kafka for event distribution at demonstrated scale.

Each addition requires a documented bottleneck, ownership model, failure behavior, and ADR.
