# ADR-0002: WebSocket-First Initial Transport

- Status: Accepted
- Date: 2026-07-31

## Context

The first playable slice requires reliable commands, snapshots, ordered terrain deltas, reconnect, and protocol inspection between a Godot client and standalone .NET server.

Potential transports include WebSocket, ENet, raw UDP with a reliability layer, and TCP-based custom framing.

## Decision

Use WebSocket for all initial gameplay messages.

Use JSON encoding behind explicit versioned shared protocol contracts. Transport and encoding remain separable so a binary encoding or additional datagram channel can be introduced later without changing authoritative domain semantics.

## Consequences

### Positive

- Reliable ordered delivery simplifies initial command and replication behavior.
- Godot and .NET have mature WebSocket support.
- Traffic is easy to inspect, log, replay, and test.
- Authentication, reconnect, and constrained-network support are straightforward.
- The project avoids premature implementation of custom reliability and congestion behavior.

### Negative

- Head-of-line blocking may be unsuitable for high-frequency transient movement at scale.
- JSON uses more bandwidth and CPU than a compact binary format.
- Reliable delivery may transmit stale transient updates unless the protocol coalesces them.

## Required Protocol Behavior

- Version negotiation during session bootstrap.
- Typed command results and rejection codes.
- Monotonic sequence per replication stream.
- Duplicate and stale message rejection.
- Gap detection and resynchronization.
- Snapshot plus tail-delta reconnect behavior.
- Message-size and rate limits.

## Alternatives Considered

### ENet or Raw UDP

Potentially appropriate for high-frequency movement and combat. Rejected for the initial terrain slice because the project has no measured transport bottleneck and reliability semantics would increase implementation risk.

### Godot High-Level RPC

Convenient for small homogeneous Godot projects. Rejected as the canonical protocol because the authoritative server is a standalone .NET service and protocol contracts must not depend on scene-tree paths or engine RPC annotations.

## Revisit Conditions

Add a datagram channel or binary encoding only after measurements identify unacceptable latency, bandwidth, allocation, or head-of-line blocking that cannot be addressed by message coalescing and interest-management budgets.
