# Pull Request Review Checklist

## Review Goal

Determine whether the change safely achieves its observable objective. Review the risks the change actually affects; do not apply every repository concern to every pull request.

## 1. Objective and Scope

- [ ] The observable result is clear.
- [ ] The implementation achieves it in the supported workflow.
- [ ] Scope changes or new dependencies are intentional and acceptable.
- [ ] No unrelated work creates material review risk.

A requirement-to-file mapping is optional. Review behavior, not paperwork completeness.

## 2. Risk Classification

Confirm the stated level:

- **Low:** docs, tooling, metadata, scaffolding, mechanical refactor, test-only.
- **Medium:** ordinary feature or subsystem change.
- **High:** persistence, transactionality, concurrency, recovery, idempotency, economy, security, protocol compatibility, authority, or architecture.

Apply the remaining checks only where relevant.

## 3. Material Correctness

- [ ] Supported success behavior works.
- [ ] Realistic failure behavior is safe and understandable.
- [ ] Public or serialized contracts remain compatible, or the change is explicit.
- [ ] Client input is not trusted as authoritative shared state.
- [ ] Tests are meaningful for the affected behavior.

## 4. High-Risk Checks

When applicable:

- [ ] Transactions and migrations preserve durable state.
- [ ] Retry and idempotency behavior cannot duplicate effects.
- [ ] Concurrency and ordering preserve ownership and sequence invariants.
- [ ] Restart, replay, and recovery converge to canonical state.
- [ ] Protocol and storage compatibility are defined.
- [ ] Security boundaries and resource limits are enforced.
- [ ] High-cost or irreversible architecture changes have an ADR.

Do not demand these sections for unrelated low-risk work.

## 5. Godot and Human Validation

When affected behavior is visual, interactive, import/export, graphics, input, audio, connection, or runtime behavior:

- [ ] The focused human procedure covers that behavior.
- [ ] The supported environment and result are recorded.
- [ ] Screenshot or video is present only when it proves the acceptance criterion.

A later commit requires retesting only when it can affect the tested behavior.

## 6. Classification of Findings

### Blocking defect

A concrete material problem: missing objective, broken supported workflow, unsafe state mutation, data loss/corruption/duplication, recovery failure, security/authority violation, incompatible contract, unapproved architecture, or a missing test for a required material risk.

### Follow-up improvement

Hardening, cleanup, broader edge cases, unsupported-platform parity, style preferences, or valuable work not required for safe completion.

Do not convert follow-ups into blockers merely because they are technically possible.

## 7. Review Behavior

- Find material blockers first.
- Prefer reproduced or realistic failures over speculative edge cases.
- After requested fixes, verify those fixes and affected areas; do not restart the entire threat model without new risk.
- Stop expanding test matrices once supported behavior and stated invariants are adequately proven.
- When the issue specification is wrong, amend it once using observed evidence.

## Review Output

```markdown
## Result

Pass / Changes required

## Blocking defects

None, or numbered concrete failures.

## Validation assessment

What is proven and what remains.

## Follow-ups

Optional non-blocking improvements.
```
