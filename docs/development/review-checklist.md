# Pull Request Review Checklist

## Review Instruction

Use this repository-specific instruction when reviewing an implementation pull request:

> Review this pull request against its linked issue, accepted ADRs, governing feature specifications, and repository invariants. Verify every acceptance criterion. Identify missing tests, scope expansion, architectural deviations, concurrency or recovery failures, insecure client trust, protocol incompatibilities, and undocumented behavior. Do not approve based only on passing CI. Distinguish blocking defects from optional improvements.

## 1. Scope and Requirement Coverage

- [ ] The pull request links exactly one implementation issue.
- [ ] Every required change in the issue is implemented.
- [ ] The PR requirement-mapping table is accurate.
- [ ] No acceptance criterion was omitted, weakened, or reinterpreted.
- [ ] Changes outside the expected-file list are justified.
- [ ] No unrelated refactor, cleanup, or dependency change is included.
- [ ] Non-goals remain excluded.

## 2. Architecture and Authority

- [ ] Accepted ADRs are followed.
- [ ] The Godot client remains presentation, input, interpolation, and local UX rather than gameplay authority.
- [ ] The standalone server remains authoritative for persistent and shared state.
- [ ] Transport code does not contain gameplay rules.
- [ ] Persistence code does not depend on presentation concerns.
- [ ] No distributed system is introduced before the plan requires it.

## 3. Contracts and Compatibility

- [ ] Public types use the required typed identifiers and coordinates.
- [ ] Serialized field names and meanings match the issue.
- [ ] Versioning and compatibility behavior are tested.
- [ ] Unknown, missing, malformed, duplicate, stale, and oversized inputs are handled as required.
- [ ] Error and rejection codes are deterministic and machine-readable.
- [ ] No ad hoc string contract bypasses an existing shared type.

## 4. Concurrency and Ordering

- [ ] Ownership of mutable state is explicit.
- [ ] Single-writer zone rules are preserved where applicable.
- [ ] Lock or ownership ordering is deterministic.
- [ ] Concurrent requests cannot double-apply state.
- [ ] Sequence values cannot duplicate or regress.
- [ ] Cancellation and shutdown do not leave partially applied state.
- [ ] Queue limits and overload behavior are defined where applicable.

## 5. Persistence, Idempotency, and Recovery

- [ ] Domain changes and required outbox/history records commit atomically.
- [ ] Same idempotency key and same request hash return the original result.
- [ ] Same idempotency key and different hash return a deterministic conflict.
- [ ] Retry behavior cannot duplicate observable state.
- [ ] Migration, rollback, and compatibility implications are documented.
- [ ] Crash points required by the issue are tested.
- [ ] Restart and replay converge to the same canonical state.
- [ ] Cached or derived state is rebuildable from canonical state.

## 6. Terrain Invariants

When applicable:

- [ ] Corner heights are the sole elevation authority.
- [ ] Every world corner has one canonical owner.
- [ ] Borrowed borders or halos are derived and rebuildable.
- [ ] Cross-owner mutations are all-or-nothing.
- [ ] Chunk and stream revisions increase exactly as specified.
- [ ] Negative coordinates and four-chunk intersections are tested.
- [ ] Derived slope and mesh data do not become competing authority.

## 7. Tests

- [ ] Tests prove behavior rather than implementation details alone.
- [ ] Success paths are covered.
- [ ] Validation and rejection paths are covered.
- [ ] Retry and duplicate paths are covered.
- [ ] Concurrency and ordering paths are covered where applicable.
- [ ] Failure and recovery paths are covered where applicable.
- [ ] Regression tests fail without the implementation or fix.
- [ ] Existing tests remain meaningful and pass.
- [ ] Timing-dependent tests avoid arbitrary sleeps when deterministic synchronization is possible.

## 8. Security and Resource Limits

- [ ] Client-provided identity, position, time, inventory, results, and revisions are validated rather than trusted.
- [ ] Message, collection, queue, payload, and request-rate limits exist where required.
- [ ] Parsing failures do not crash the process or expose sensitive data.
- [ ] Logs avoid credentials, tokens, or private payloads.
- [ ] Administrative or debug behavior cannot be reached accidentally in production configuration.

## 9. Godot and Human Validation

When applicable:

- [ ] Project files import with the pinned Godot version.
- [ ] Scenes and resources have stable paths and references.
- [ ] Node lifecycle and signal connections do not leak or duplicate behavior.
- [ ] Main-thread and background-thread boundaries are safe.
- [ ] Human test instructions are exact and reproducible.
- [ ] The human test report names the exact head commit.
- [ ] Screenshots, video, or logs prove the relevant behavior.
- [ ] Any change after human testing was either irrelevant to the test or caused a retest.

## 10. Review Result

The final review should use one disposition:

### Approve

All required behavior is implemented and evidenced. No blocking defect remains.

### Comment

No blocking defect remains, but optional follow-up work or clarification is recorded.

### Request changes

List each blocking defect separately and connect it to a requirement, invariant, or concrete failure mode. Avoid vague comments such as "needs cleanup" or "not production ready."

## Review Output Format

```markdown
## Requirement coverage

Complete / Incomplete

## Blocking defects

1. ...

## Missing or inadequate tests

1. ...

## Architecture and risk findings

1. ...

## Human validation assessment

Sufficient / Insufficient / Not required

## Optional follow-ups

1. ...

## Disposition

Approve / Comment / Request changes
```
