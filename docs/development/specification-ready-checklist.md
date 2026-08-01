# Specification-Ready Checklist

Use this checklist only when a separate implementation issue is useful.

## All issues

- [ ] The objective describes one observable result.
- [ ] The risk level is reasonable.
- [ ] Blocking dependencies are known.
- [ ] Automated acceptance criteria prove the supported non-visual workflow.
- [ ] An editor check is identified only when visible or interactive Godot behavior needs inspection.
- [ ] No unresolved architecture decision is being delegated accidentally.

The implementation agent owns all command-line checks, including PowerShell and Windows tooling invoked from WSL. Do not assign terminal validation to a human.

An editor check needs only a sentence describing what the merger should open and inspect. It does not need an evidence plan, report, screenshot, environment record, or tested SHA.

## Medium-risk additions

- [ ] Important interfaces and compatibility constraints are clear.
- [ ] Realistic failure behavior is covered.
- [ ] Relevant governing documents are linked when useful.

## High-risk additions

When the work affects persistence, transactions, concurrency, recovery, idempotency, economy, security, protocol compatibility, authority, or architecture:

- [ ] Applicable invariants are explicit.
- [ ] Retry, failure, and recovery semantics are defined.
- [ ] Compatibility or migration behavior is defined.
- [ ] Resource or security limits are defined where needed.
- [ ] An ADR exists for high-cost or irreversible decisions.

Expected-file lists, exhaustive non-goals, evidence plans, stop conditions, and universal risk sections are optional.