# Label Taxonomy

Labels help coordination; they are not prerequisites for small, obvious work.

## Status

- `status:planned`
- `status:ready`
- `status:in-progress`
- `status:review`
- `status:human-test`
- `status:blocked`

## Area

- `area:client`
- `area:server`
- `area:protocol`
- `area:persistence`
- `area:networking`
- `area:terrain`
- `area:infrastructure`
- `area:testing`

## Risk

- `risk:low`: documentation, tooling, metadata, scaffolding, mechanical, or test-only work.
- `risk:medium`: ordinary feature or subsystem work.
- `risk:high`: persistence, concurrency, recovery, protocol, security, authority, economy, or architecture-sensitive work.

## Agent Routing

- `agent:small`
- `agent:standard`
- `agent:strong`

## Usage

- Apply labels when they improve planning, filtering, delegation, or dependency tracking.
- Medium/high-risk milestone issues should normally have status, area, risk, and routing labels.
- Low-risk pull requests do not need labels merely to satisfy process.
- Use `status:blocked` only for a concrete named dependency or decision.
- Use `status:human-test` only when human behavior validation is the remaining gate.
- `risk:high` work should normally receive explicit review of the high-risk concern and use an appropriately capable agent.
