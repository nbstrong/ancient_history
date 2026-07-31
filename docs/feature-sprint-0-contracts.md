# Feature Specification: Sprint 0 Delivery Semantics

## Status

Planned. Prototype interfaces exist, but the acceptance criteria in this document are not yet satisfied.

## Purpose

Define and implement the cross-cutting contracts required before authoritative terrain or action systems are built. Sprint 0 establishes identity, protocol compatibility, idempotency, transaction boundaries, stream sequencing, and commit-to-replication reliability.

## Scope

- Validated canonical identifier and coordinate types.
- Protocol envelope and compatibility policy.
- Typed command result and rejection contracts.
- Idempotent mutating-command behavior.
- PostgreSQL transaction abstraction.
- Transactional outbox and stream sequence allocation.
- Outbox publication and replay.
- Single-writer zone command-queue skeleton.
- Contract, concurrency, and failure-injection tests.

## Non-Goals

- Terrain data structures or mutation rules.
- Production authentication platform.
- Full movement replication.
- Binary protocol optimization.
- Multi-node zone ownership.
- Redis, Kafka, NATS, or object storage.

## Functional Requirements

## 1. Identifier Contract

Define validated types for at least:

- `ChunkId`
- `ZoneId`
- `EntityId`
- `ActionId`
- `ItemId`
- `ContainerId`
- `ActorId`
- `MutationId`
- `CorrelationId`
- `IdempotencyKey`

Define monotonic value types for:

- `ChunkRevision`
- `EntityRevision`
- `ContainerRevision`
- `StreamSequence`

Requirements:

- Empty and malformed values are rejected at construction or boundary parsing.
- Identifiers have stable string and JSON representations.
- Database representations are explicit.
- No authoritative subsystem uses ad hoc string identifiers after this contract is adopted.

## 2. Coordinate Contract

Define explicit coordinate types:

- `WorldTile`
- `WorldCorner`
- `ChunkCoordinate`
- `LocalTile`
- `LocalCorner`

Requirements:

- Negative world coordinates are supported.
- Conversion and ownership rules are deterministic.
- Wire payloads do not encode coordinates as comma-separated strings.
- Multi-corner commands use typed records rather than parallel arrays.

## 3. Protocol Envelope Contract

Every message includes:

- `protocol_version_major`
- `protocol_version_minor`
- `message_type`
- `stream_id`
- `sequence`
- `correlation_id`
- `sent_at`
- `payload`

Compatibility policy:

- Minor-version changes are additive.
- Fields are not removed or renamed within a major version.
- Unknown additive fields are ignored safely.
- Unsupported message types receive a typed rejection.
- Breaking changes require a major-version increment.
- Session bootstrap negotiates supported versions before gameplay messages are accepted.

## 4. Command Result Contract

Every mutating command returns one typed outcome:

- Success result.
- Domain rejection.
- Stale-revision conflict.
- Idempotency conflict.
- Authorization failure.
- Unsupported protocol or message failure.
- Temporary server-unavailable failure.

Rejection reason codes are stable and safe to expose to clients. Internal exception details are logged but not serialized to clients.

## 5. Idempotency Contract

Every mutating command includes:

- `actor_id`
- `idempotency_key`
- `request_hash`

Uniqueness scope:

`(actor_id, idempotency_key)`

Required behavior:

- First valid request establishes the stored request hash and final result.
- Same key and same hash returns the original result.
- Same key and different hash returns deterministic `IdempotencyConflict`.
- Concurrent first requests produce one winner through a database uniqueness constraint.
- No domain side effect is applied twice.
- Retention is at least 24 hours.
- Economy-sensitive commands may use longer retention.

The request hash is generated from a canonical command representation, not arbitrary raw JSON formatting.

## 6. Transaction Contract

`ITransactionRunner` provides an explicit PostgreSQL transaction context to all repositories participating in a command.

Requirements:

- Domain changes, audit history, stream sequence allocation, outbox records, and stored command result commit atomically.
- Repositories do not silently open independent transactions inside the command transaction.
- Cancellation before commit rolls back.
- Ambiguous commit outcomes are resolved by querying the idempotency record before retrying.

## 7. Transactional Outbox Contract

Outbox records include:

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

Requirements:

- Domain state and outbox append commit in one transaction.
- `(stream_id, sequence)` is unique.
- Sequence is allocated in the same transaction.
- Transport is at least once.
- Observable application is exactly once through sequence and dedupe handling.
- Unpublished records survive server restart.
- Publication order is preserved within each stream.

## 8. Replication Stream Contract

Initial stream formats include:

- `session:{session_id}`
- `actor:{actor_id}`
- `zone:{zone_id}`
- `chunk:{chunk_id}`
- `container:{container_id}`

Sequence rules:

- Unsigned 64-bit monotonic value.
- Begins at `1`.
- `sequence <= last_applied` is duplicate or stale and is dropped.
- `sequence == last_applied + 1` is applied.
- A larger gap triggers resynchronization.

Reconnect behavior:

- Client reports last applied sequence by subscribed stream.
- Server replays retained deltas when possible.
- Otherwise, server sends a fresh snapshot followed by tail deltas.

## 9. Zone Execution Contract

- A `ZoneExecutor` is the exclusive in-process writer for zone-local state.
- Network handlers enqueue validated commands and do not mutate world state.
- Commands execute in deterministic queue order.
- Cross-zone authority is acquired in stable ascending zone order or handled by an explicit coordinator.
- Queue depth, command wait, and tick duration are observable.

This model replaces broad reliance on general-purpose in-process locks for ordinary zone-local mutations.

## Minimum Database Schema

### `action_history`

- `action_id`
- `actor_id`
- `idempotency_key`
- `request_hash`
- `command_type`
- `status`
- `result_payload`
- `result_schema_version`
- `created_at`
- `completed_at`
- `expires_at`

Unique constraint:

- `(actor_id, idempotency_key)`

### `stream_sequences`

- `stream_id`
- `next_sequence`

### `action_outbox`

- Fields defined in the outbox contract.

Unique constraints:

- `(stream_id, sequence)`
- Dedupe key within its defined event scope.

## Required Interfaces

- `ITransactionRunner`
- `IIdempotencyRepository`
- `IStreamSequenceRepository`
- `IOutboxRepository`
- `IOutboxPublisher`
- `IZoneCommandRouter`

In-memory implementations must be named and documented as test doubles. Production paths use PostgreSQL-backed implementations.

## Acceptance Criteria

### Build and Contract

- Shared contracts compile independently of Godot.
- Client and server reference the same shared contracts.
- Malformed identifiers and coordinates are rejected.
- Current protocol round trips without data loss.
- Compatibility tests cover additive minor fields and unsupported major versions.

### Idempotency

- `0/10,000` duplicate side-effect applications under concurrent same-hash retries.
- Same key and different hash always returns conflict with `0` side effects.
- Stored result is returned after process restart.
- Retention-expiry boundary behavior is tested.

### Transaction and Outbox

- Domain state, result, sequence, and outbox append are atomic.
- Crash after commit but before publish results in later publication.
- Duplicate publication produces one observable client application.
- Per-stream order is preserved.
- Outbox lag and failure metrics are available.

### Stream Handling

- Duplicate and stale messages are dropped.
- Gap detection triggers replay or snapshot resynchronization.
- Reconnect restores the expected stream cursor.

### Zone Execution

- Zone-local mutations have one in-process writer.
- Queue ordering and shutdown behavior are deterministic.
- Reverse authority acquisition is detected in tests.

## Test Plan

### Unit

- Identifier parse, format, and malformed-input tests.
- Coordinate conversion tests including negative positions.
- Canonical request-hash tests.
- Protocol compatibility tests.
- Client stream cursor tests.

### Integration

- PostgreSQL idempotency tests.
- Transactional sequence and outbox tests.
- Publisher retry and restart tests.
- Session reconnect tests.

### Concurrency

- Concurrent identical commands.
- Concurrent same-key/different-hash commands.
- Concurrent sequence allocation on one stream.
- Parallel publication across independent streams.

### Failure Injection

Inject failure after:

- Idempotency row insert.
- Domain update.
- Sequence allocation.
- Outbox append.
- Database commit.
- First publication attempt.
- Published-marker update.

Every failure point must converge to a documented state after retry or restart.

## Deliverables

- Hardened shared types and protocol files.
- PostgreSQL migrations and repositories.
- Outbox publisher.
- Zone command-queue skeleton.
- Automated contract, concurrency, and failure-injection tests.
- Documentation updated to match implemented behavior.
