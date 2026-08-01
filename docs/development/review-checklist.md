# Pull Request Review Checklist

## Review goal

Determine whether the change safely achieves its observable objective. Review only the risks the change actually affects.

## Objective and scope

- [ ] The observable result is clear.
- [ ] The implementation achieves it in the supported workflow.
- [ ] Scope changes or new dependencies are intentional and acceptable.
- [ ] No unrelated work creates material review risk.

A requirement-to-file mapping is optional. Review behavior, not paperwork.

## Material correctness

- [ ] Supported success behavior works.
- [ ] Realistic failure behavior is safe and understandable.
- [ ] Public or serialized contracts remain compatible, or the change is explicit.
- [ ] Client input is not trusted as authoritative shared state.
- [ ] Automated tests are meaningful for the affected behavior.

## High-risk checks

Only when applicable:

- [ ] Transactions and migrations preserve durable state.
- [ ] Retry and idempotency behavior cannot duplicate effects.
- [ ] Concurrency and ordering preserve ownership and sequence invariants.
- [ ] Restart, replay, and recovery converge to canonical state.
- [ ] Protocol and storage compatibility are defined.
- [ ] Security boundaries and resource limits are enforced.
- [ ] High-cost or irreversible architecture changes have an ADR.

## Validation ownership

The implementation agent owns all command-line validation, including Bash, .NET, PowerShell and Windows tools invoked from WSL, and headless Godot checks.

Do not request that a human rerun terminal commands or provide copied output, screenshots, environment records, or tested-SHA attestations.

## Editor check

When visible or interactive Godot behavior changed, the pull request should say what the merger needs to inspect in the editor.

No evidence is required. The reviewer only confirms that the requested editor check is focused and understandable. Merging means the check was accepted.

## Findings

### Blocking defect

A concrete material problem: missing objective, broken supported workflow, unsafe state mutation, data loss/corruption/duplication, recovery failure, security/authority violation, incompatible contract, unapproved architecture, or missing automated coverage for a material affected risk.

### Follow-up improvement

Hardening, cleanup, broader edge cases, unsupported-platform parity, style preferences, or valuable work not required for safe completion.

Do not convert follow-ups into blockers merely because they are technically possible.

## Review behavior

- Find material blockers first.
- Prefer reproduced or realistic failures over speculative edge cases.
- After requested fixes, verify those fixes and affected areas; do not restart the entire threat model without new risk.
- Stop expanding test matrices once supported behavior and stated invariants are adequately proven.
- When the issue specification is wrong, amend it once using observed evidence.

## Review output

```markdown
## Result

Pass / Changes required

## Blocking defects

None, or numbered concrete failures.

## Automated validation assessment

What is proven and what remains.

## Editor check

Not needed, or what the merger should inspect.

## Follow-ups

Optional non-blocking improvements.
```