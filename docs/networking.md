# Networking and Replication

## Protocol Strategy

- Transport: UDP + reliability layer for action-critical messages.
- Optional fallback: WebSocket/TCP for constrained environments.
- Message schema versioned (e.g., protobuf/flatbuffers/custom binary schema).

## Message Categories

1) Reliable Ordered
- Login/auth/session control.
- Inventory transactions.
- Action start/cancel/complete.

2) Reliable Unordered
- Chat, social notifications, economy updates.

3) Unreliable Sequenced
- Entity movement snapshots.
- Non-critical environment updates.

## Interest Management

- AOI based on player position + visibility radius.
- Prioritization tiers:
  - Tier 0: self, combat targets, active action targets.
  - Tier 1: nearby players/creatures.
  - Tier 2: distant/static objects.
- Snapshot budget per client per tick with priority fill.

## Replication Model

- Entity component snapshots with dirty-bit delta compression.
- Periodic full state keyframe for correction.
- Client interpolation for remote actors.
- Server reconciliation for controlled entities.

## Anti-Cheat Validation

- Movement validation:
  - Speed, acceleration, terrain constraints.
- Action validation:
  - Range checks, tool requirements, cooldown windows.
- Economic validation:
  - Signed transaction IDs, idempotency keys.

## Shard/Zone Transition

- Client receives pre-transition token and next-node endpoint.
- Dual subscription window for seamless crossing.
- Finalize transition when both server and client acknowledge state sync tick.

## Observability

Track:
- RTT/jitter/loss by region.
- Snapshot size and dropped update rates.
- Command rejection reasons.
- Handoff success/failure metrics.
