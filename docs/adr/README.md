# Architecture Decision Records

Architecture Decision Records document choices that are expensive to reverse or that constrain multiple subsystems.

## Status Values

- Proposed: under review and not yet binding.
- Accepted: current project direction.
- Superseded: replaced by a later ADR.
- Rejected: considered and intentionally not selected.

## Records

- [ADR-0001: Standalone .NET Authoritative Server](0001-standalone-dotnet-server.md)
- [ADR-0002: WebSocket-First Initial Transport](0002-websocket-first-transport.md)
- [ADR-0003: Single-Writer Zone Execution](0003-single-writer-zones.md)
- [ADR-0004: Canonical Terrain Corner Ownership](0004-canonical-corner-ownership.md)

## Required ADR Topics Before Expansion

Create additional ADRs before committing to:

- Binary protocol encoding.
- Datagram movement transport.
- Multi-node zone handoff.
- External cache or distributed lock service.
- External durable event bus.
- Object storage for snapshots.
- Cave and underground terrain representation.
- Economy ledger architecture.
