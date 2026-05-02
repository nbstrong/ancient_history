# Roadmap and Milestones

## Milestone 0: Foundations (2-4 weeks)

Goals:
- Headless server bootstrap.
- Basic login/session + one test map.
- Authoritative movement with AOI snapshots.
- Minimal persistence for character location/inventory.

Exit Criteria:
- 20 concurrent test clients stable for 1 hour.
- No desync in authoritative position after 10k movement commands.

## Milestone 1: Vertical Slice (4-8 weeks)

Goals:
- End-to-end gather -> process -> craft loop.
- 5-10 skills, 30-50 item definitions.
- Terrain dig/level MVP and persistence.
- Basic creature AI and combat prototype.

Exit Criteria:
- Fresh character can produce a crafted tool from raw materials.
- Terrain edits survive restart.
- No item duplication in stress test suite.

## Milestone 2: Settlement and Economy (6-10 weeks)

Goals:
- Deeds/settlement permissions.
- Structure planning/building/upkeep.
- Player-to-player trade and market board.
- Decay systems and repair/improve loops.

Exit Criteria:
- Two-player settlement workflow works end-to-end.
- Economy ledger balances under concurrency tests.

## Milestone 3: Scale and Hardening (6-12 weeks)

Goals:
- Zone handoff between world nodes.
- Performance tuning and interest-management optimization.
- Observability dashboards + on-call alerts.
- Security hardening and exploit regression suite.

Exit Criteria:
- 200+ concurrent users in soak test target environment.
- Node failover recovery without durable state corruption.

## Milestone 4: Alpha Operations

Goals:
- Closed alpha content pass.
- Live-ops tooling, GM commands, moderation workflows.
- Patch pipeline and migration automation.

Exit Criteria:
- Weekly patch cadence proven over at least 4 releases.
- Incident response runbook validated.

## Cross-Cutting Backlog

- Accessibility and UX pass for dense MMO interfaces.
- Localization pipeline.
- Bot/fraud detection.
- Community/social features (guilds, mail, events).
