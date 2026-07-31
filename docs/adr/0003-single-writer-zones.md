# ADR-0003: Single-Writer Zone Execution

- Status: Accepted
- Date: 2026-07-31

## Context

The authoritative world contains terrain chunks, entities, actions, and containers that may be accessed concurrently by many sessions. A general shared-memory locking model would spread lock acquisition and deadlock risk across gameplay systems.

The world is already conceptually partitioned into chunks and zones for interest management and future scaling.

## Decision

Use a single-writer executor for zone-local authoritative state wherever practical.

Network handlers decode, authenticate, perform bounded message validation, and enqueue commands. They do not directly mutate domain state. Each zone executor processes its command queue in deterministic order under an authoritative world clock.

Cross-zone operations use stable ascending zone order or a dedicated coordinator with explicit transaction and failure semantics.

## Consequences

### Positive

- Ordinary zone-local mutations do not require broad in-process locking.
- Command order, tests, metrics, and failure analysis are easier to reason about.
- Zones form a natural future distribution boundary.
- Tick duration and queue pressure are directly observable.
- Domain code is less likely to deadlock through inconsistent lock acquisition.

### Negative

- A slow command can delay other work in the same zone unless expensive work is split or scheduled carefully.
- Cross-zone operations require explicit coordination.
- Queue capacity, backpressure, cancellation, and shutdown behavior must be designed.
- Database concurrency constraints remain necessary for cross-zone and recovery integrity.

## Rules

- One active in-process writer owns zone-local canonical state.
- Commands have bounded queue sizes and payloads.
- Expensive non-authoritative computation may run elsewhere, but final validation and commit return to zone authority.
- Database commit is the durable boundary.
- Zone state is not acknowledged successful before durable commit.
- Metrics include queue depth, queue wait, tick duration, command duration, and rejection counts.

## Alternatives Considered

### Fine-Grained Locks Throughout Domain Code

Provides parallelism but creates complex lock ordering and failure behavior across chunks, entities, and containers. Rejected as the default model.

### One Global World Thread

Simple but creates an unnecessary global bottleneck and a poor future scaling boundary. Rejected in favor of zone-local executors, even while all zones initially run in one process.

### Actor Per Entity

Potentially useful for specific high-concurrency entities, but introduces message and lifecycle overhead and complicates multi-entity transactions. Not selected as the initial general model.

## Revisit Conditions

Revisit zone size, executor scheduling, or selective parallelism after profiling shows a sustained zone bottleneck and the correctness suite can validate any new concurrency model.
