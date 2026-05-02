# Feature Document: Sprint 0 Contracts (v2)

## Feature Name

Shared Contracts and Delivery Semantics Bootstrap

## Purpose

Define the cross-cutting contracts required before Step 1 terrain implementation, so IDs, protocol payloads, sequencing, idempotency, and commit-to-replication behavior are consistent across server and client.

## Why This Exists

`feature-step-1-terrain-core.md` specifies terrain behavior. This Sprint 0 feature specifies the shared contract layer terrain and all later features depend on.

## Scope

- Canonical ID types and formatting rules.
- Message envelope and protocol versioning rules.
- Correlation and idempotency fields.
- Transactional outbox contract for commit -> replicate reliability.
- Minimal interface contracts for zone authority and lock ordering.
- Contract alignment with Step 1 terrain persistence schema.

## Non-Goals

- Full terrain implementation.
- Full replication implementation.
- Full database schema rollout beyond required contract tables/interfaces.

## Functional Requirements

1) ID Contract
- Define stable IDs for at least:
  - `ChunkId`, `ZoneId`, `EntityId`, `ActionId`, `ItemId`, `ActorId`, `MutationId`.
- Define typed monotonic counters:
  - `ChunkRevision` (per chunk)
  - `StreamSequence` (per replication stream)
- IDs and counters must be serializable and safe for storage/network transport.
- No subsystem may use ad-hoc string IDs once this contract is in place.

2) Protocol Envelope Contract
- Every network message includes:
  - `protocol_version_major`
  - `protocol_version_minor`
  - `message_type`
  - `stream_id`
  - `sequence`
  - `correlation_id`
  - `sent_at`
  - `payload`
- Compatibility policy:
  - Minor version changes are additive only.
  - No field rename/removal within same major version.
  - Unknown fields must be ignored safely.
  - Breaking changes require major version bump.

3) Idempotency Contract
- Mutating commands include `idempotency_key`.
- Uniqueness scope is `(actor_id, idempotency_key)`.
- Store `request_hash` with first-seen request.
- If same key arrives with different hash, return deterministic conflict error and do not apply side effects.
- Retry with same key + same hash must return original result, never reapply side effects.
- Retention window minimum: 24 hours.

4) Commit/Replicate Contract
- Server commit path writes gameplay state + outbox record in one transaction.
- Replication emits only from committed outbox records.
- Outbox records include monotonic sequence per stream.
- Delivery semantics: at-least-once transport; idempotent apply yields exactly-once observable state.
- Outbox payload includes dedupe key (`mutation_id` or `action_id` + `sequence`).

5) Replication Stream Contract
- `stream_id` format: `zone:{zone_id}` or `chunk:{chunk_id}`.
- Sequence type: unsigned 64-bit monotonic integer.
- Sequence starts at `1` for new stream.
- Gaps are treated as recoverable desync and trigger stream resync (snapshot + replay window request).
- Reorder/out-of-date messages (`sequence <= last_applied`) are dropped.
- Reconnect behavior:
  - Client sends last applied sequence per stream.
  - Server sends delta replay when available, else snapshot then tail deltas.

6) Authority and Lock Ordering Contract
- `ZoneManager` is lock-scope owner for zone resources.
- Global lock order:
  - Primary: resource class `chunk -> entity -> container`.
  - Secondary within class: stable key ascending (e.g., `ChunkId`, `EntityId`, `ContainerId`).
- Violations are test failures.

## Data/Schema Contract (Minimum)

Step 1 terrain persistence remains authoritative in `feature-step-1-terrain-core.md`. Sprint 0 must include compatible contract definitions for these tables:
- `terrain_chunks`
- `terrain_mutation_events`
- `terrain_mutation_chunks`

Sprint 0-specific contract tables:

- `action_outbox`
  - `outbox_id`
  - `stream_id`
  - `sequence`
  - `event_type`
  - `dedupe_key`
  - `payload_json`
  - `created_at`
  - `published_at` (nullable)

- `action_history`
  - `action_id`
  - `actor_id`
  - `idempotency_key`
  - `request_hash`
  - `status`
  - `result_payload`
  - `created_at`

## API/Interface Contract (Minimum)

- `ITransactionRunner`
  - `RunInTransaction(Func<Task>)`

- `IOutboxRepository`
  - `Append(OutboxEvent e)`
  - `ReadUnpublished(streamId, limit)`
  - `MarkPublished(outboxId, publishedAt)`

- `IIdempotencyStore`
  - `TryGet(actorId, idempotencyKey)`
  - `Put(actorId, idempotencyKey, requestHash, result)`

## Acceptance Criteria

- Shared contract types compile and are referenced by terrain/action modules.
- Envelope serialization/deserialization tests pass for current major/minor protocol versions.
- Idempotency retry test shows `0/10,000` duplicate side-effect applications.
- Same idempotency key with different payload hash always returns conflict and `0` side-effect applies.
- Crash simulation between DB commit and publish shows at-least-once outbox replay and exactly-once observable state after restart.
- Lock ordering tests confirm no reverse-order acquisitions.

## Test Plan

- Unit:
  - ID parse/format stability tests.
  - Malformed ID rejection tests.
  - Envelope round-trip tests.
  - Version compatibility tests (unknown field tolerance, major/minor policy).
  - Lock-order guard tests.

- Integration:
  - Transactional outbox crash-recovery test.
  - Idempotency retry and replay test.
  - Idempotency key + hash mismatch conflict test.
  - Stream sequence gap/reorder handling tests.
  - Retention expiry boundary test for idempotency records.

## Dependencies

- Baseline server project and test harness.
- Persistence access layer scaffolding.

## Risks

- Contract churn if message/version fields are not frozen early.
- Replication bugs if outbox stream semantics are underspecified.

## Deliverables

- `src/shared/Types/Ids.cs`
- `src/shared/Protocol/Messages.cs`
- `src/server/Persistence/Db.cs`
- `src/server/Actions/ActionOutbox.cs`
- `src/server/Persistence/Repositories/OutboxRepository.cs`
- Minimum contract tests in `tests/`
