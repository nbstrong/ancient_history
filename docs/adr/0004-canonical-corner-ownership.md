# ADR-0004: Canonical Terrain Corner Ownership

- Status: Accepted
- Date: 2026-07-31

## Context

Wurm-style terrain stores elevation at tile corners. Chunk boundaries therefore share logical corner positions. Persisting writable copies in every adjacent chunk creates competing authority, seam divergence, and ambiguous recovery.

The client and simulation still benefit from a complete logical corner grid around each chunk.

## Decision

Every world-space terrain corner has exactly one canonical owner chunk.

Owner selection is a deterministic versioned function of world corner coordinates and chunk dimensions. Neighbor chunks may maintain borrowed border or halo values for efficient reads, snapshot assembly, and mesh generation, but those values are rebuildable caches and are not competing persistent authority.

Mutations targeting a borrowed corner are routed to the canonical owner. Mutations affecting multiple owners commit atomically.

## Consequences

### Positive

- One durable source of truth exists for every corner.
- Seam consistency becomes a cache-integrity check rather than conflict resolution between duplicate records.
- Snapshot and replay behavior is unambiguous.
- Border mutation tests can prove owner uniqueness and halo agreement.
- Storage avoids authoritative duplicate values.

### Negative

- Chunk snapshots or client payloads may need to assemble logical borders from neighboring owners.
- Owner resolution must handle negative coordinates and exact boundaries correctly.
- Multi-owner mutations require explicit authority coordination and atomic persistence.
- Finite-world edge behavior must be defined if the world is bounded.

## Rules

- Canonical owner resolution is pure, deterministic, and versioned.
- Every world corner resolves to exactly one owner.
- Borrowed values are never written as independent canonical state.
- Borrowed halos are rebuilt after recovery and affected mutations.
- A halo mismatch is detected and reported.
- Multi-owner mutation revisions and deltas commit in one PostgreSQL transaction.
- Property tests cover interiors, edges, four-chunk junctions, and negative coordinates.

## Alternatives Considered

### Duplicate Writable Border Corners

Simple local access but requires two or four records to remain perfectly synchronized through failures and replay. Rejected because it creates multiple authorities for one value.

### Store a Global Unchunked Corner Grid

Provides unique authority but makes partitioned loading, persistence, and future zone ownership less direct. Rejected for the initial chunk-oriented world model.

### Separate Corner Entities or Tables for Every Coordinate

Provides explicit identity but may introduce excessive row and lookup overhead for dense terrain. Not selected for the initial snapshot-plus-mutation design.

## Revisit Conditions

The exact owner formula must be finalized before terrain implementation. The canonical-owner principle remains unless a replacement demonstrates equivalent uniqueness, recovery, and performance properties through an ADR and the full property-test suite.
