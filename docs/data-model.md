# Data Model and Persistence

## Storage Layers

- PostgreSQL for canonical transactional state.
- Redis for hot caches and ephemeral locks.
- Snapshot blobs for chunk/world recovery points.
- Append-only event stream for audit and replay (phase 2+).

## Core Domain Tables (Initial)

- `accounts`
- `characters`
- `skills`
- `items`
- `item_instances`
- `inventories`
- `inventory_slots`
- `containers`
- `terrain_chunks`
- `terrain_mutations`
- `structures`
- `settlements`
- `permissions`
- `creatures`
- `market_orders`
- `trade_transactions`
- `action_history`

## Transaction Patterns

- Inventory move/craft/build all use DB transactions with:
  - Row-level locks on source/destination containers.
  - Idempotency key per command.
  - Append audit entry on success/failure.

## Serialization Rules

- Use stable UUIDs for entity identity.
- Never reuse IDs after deletion.
- Version every persisted payload with migration path.

## Recovery Strategy

- Point-in-time backup for PostgreSQL.
- Regular chunk snapshots + mutation log shipping.
- Recovery test cadence: at least monthly full restore drill.

## Data Integrity Checks

- Nightly jobs:
  - Duplicate ownership scan for item instances.
  - Invalid container graph scan (cycles/orphans).
  - Terrain mutation continuity checks per chunk.
