# Networking and Replication

## Purpose

This document defines the initial client-server networking model for the first playable vertical slice. The design favors debuggability, correctness, reconnect behavior, and protocol stability over premature transport optimization.

## Initial Transport Decision

Use WebSocket as the first transport for all gameplay messages.

Initial encoding is JSON using explicit shared contracts. The transport and encoding are separate concerns: JSON may later be replaced by a compact binary encoding without changing authoritative gameplay semantics.

Reasons for the initial choice:

- Works cleanly between a Godot client and standalone .NET server.
- Reliable ordered delivery simplifies the first terrain and action slices.
- Easy packet inspection and deterministic integration testing.
- Straightforward support for authentication, reconnect, and constrained networks.
- Avoids building a custom UDP reliability layer before traffic measurements exist.

A sequenced datagram channel may be introduced later for high-frequency movement or transient state after profiling demonstrates a need.

## Protocol Independence

The canonical protocol must not depend on Godot node paths, scene trees, or engine RPC annotations.

Shared protocol types define:

- Message envelope.
- Command payloads.
- Result and rejection payloads.
- Snapshot and delta payloads.
- Sequence and revision fields.
- Compatibility behavior.

Godot-specific code adapts these contracts to client scenes and views.

## Message Envelope

Every message includes:

- `protocolVersionMajor`
- `protocolVersionMinor`
- `messageType`
- `streamId`
- `sequence`
- `correlationId`
- `sentAt`
- `payload`

Mutating commands additionally include:

- `actorId`
- `idempotencyKey`
- `requestHash`
- Expected entity, chunk, or container revisions where applicable.

Server responses include either a typed success result or a typed rejection with a stable reason code.

## Compatibility Policy

- Minor versions are additive.
- Existing fields are not renamed or removed within a major version.
- Unknown additive fields are ignored safely.
- Unknown message types are rejected with a stable protocol error.
- Breaking changes require a major-version increment.
- The server declares the supported major version and minimum compatible minor version during session bootstrap.

Protocol compatibility tests are required before changing shared message contracts.

## Message Classes

### Commands

Client intent that may mutate authoritative state.

Examples:

- Begin, cancel, or continue an action.
- Apply a terrain mutation.
- Move an item.
- Deliver construction material.

Commands require authentication, authorization, validation, and idempotency handling.

### Results

Direct response to a command.

A result includes:

- Correlation identifier.
- Idempotency outcome.
- Authoritative revisions.
- Typed result payload or rejection reason.

A duplicate command with the same actor, idempotency key, and request hash returns the stored original result.

### Snapshots

Full authoritative state for a subscribed stream at a revision and sequence watermark.

Examples:

- Terrain chunk snapshot.
- Zone entity snapshot.
- Player inventory snapshot.
- Active action snapshot.

Snapshots must be sufficient to initialize or repair client state without relying on prior messages.

### Deltas

Incremental committed changes following a snapshot or prior delta.

Deltas are emitted only from committed outbox records and include:

- Stream identifier.
- Monotonic sequence.
- Dedupe key.
- Authoritative revision.
- Typed change payload.

## Stream Model

Initial stream identifiers use explicit formats such as:

- `session:{sessionId}`
- `actor:{actorId}`
- `zone:{zoneId}`
- `chunk:{chunkId}`
- `container:{containerId}`

Each stream has an unsigned 64-bit monotonic sequence beginning at `1`.

Client rules:

- Apply `sequence == lastApplied + 1`.
- Drop duplicates or stale messages where `sequence <= lastApplied`.
- Treat a gap as desynchronization.
- Request replay or a fresh snapshot after a gap.

Server rules:

- Allocate sequences transactionally with the domain mutation and outbox record.
- Preserve stream order during publication.
- Retain a bounded replay window where practical.
- Fall back to snapshot plus tail deltas when replay is unavailable.

## Session Bootstrap

A session starts with:

1. WebSocket connection.
2. Protocol negotiation.
3. Authentication and session establishment.
4. Character or actor selection.
5. Initial subscription set.
6. Snapshot delivery.
7. Transition to live ordered deltas.

The client does not enter an interactive state until required initial snapshots are applied.

## Reconnect and Resynchronization

On reconnect, the client sends:

- Session or refresh credentials.
- Actor identifier.
- Last applied sequence for relevant streams.
- Last known authoritative revisions where useful.

The server responds with one of:

- Delta replay from the requested sequence.
- Fresh snapshot followed by tail deltas.
- Stream replacement when ownership or subscription scope changed.

Reconnect behavior must be tested with disconnects at every commit and publish boundary.

## Interest Management

Initial area-of-interest behavior is server-calculated from actor position and visibility rules.

Priority tiers:

- Tier 0: self, active action targets, directly controlled objects.
- Tier 1: nearby players, creatures, and interactive entities.
- Tier 2: static structures and terrain changes within view range.
- Tier 3: distant or low-priority state that may be delayed or summarized.

The server enforces a per-client replication budget. Correctness-critical events are never discarded; lower-priority state may be deferred and later repaired by snapshot.

## Movement

Movement is not part of the first terrain persistence milestone beyond basic session positioning.

When authoritative movement is implemented:

- Client sends input or movement intent, not trusted position.
- Server validates speed, acceleration, collision, terrain slope, and action constraints.
- Client predicts local presentation.
- Server sends authoritative corrections and remote snapshots.

WebSocket remains acceptable until measured movement latency or bandwidth requires a separate datagram channel.

## Outbox Publication

Replication consumes committed outbox records.

Required properties:

- Domain state and outbox record commit atomically.
- Publisher may deliver an event more than once.
- Client application is idempotent by stream sequence and dedupe key.
- `publishedAt` is operational metadata, not proof that every client received the event.
- Outbox lag and retry counts are observable.

## Security and Abuse Controls

- Maximum message size by message class.
- Strict payload validation and bounded collections.
- Session and actor command-rate limits.
- Per-command authorization.
- Server-side range, tool, revision, and ownership checks.
- Stable rejection codes without exposing sensitive server internals.
- Disconnect policy for malformed, abusive, or unsupported traffic.

## Observability

Track at minimum:

- Active and reconnecting sessions.
- Authentication and protocol negotiation failures.
- RTT and application-level command latency.
- Messages and bytes per client by class.
- Command rejection reasons.
- Stream sequence gaps and resync counts.
- Snapshot size and generation latency.
- Outbox publication lag and retry counts.
- Per-client replication queue depth.

## Deferred Networking Features

The following are deferred until the vertical slice is correct and profiled:

- Custom UDP reliability.
- Multiple transport classes.
- Binary compression and bit packing.
- Cross-node session handoff.
- Global gateway routing.
- Multi-region edge termination.

Each addition requires measured justification and an ADR.
