# Specification-Ready Checklist

Use this checklist only when a separate implementation issue is needed.

## All issues

- [ ] The objective describes one observable result.
- [ ] The risk level is reasonable.
- [ ] Blocking dependencies are known.
- [ ] Acceptance criteria prove the supported workflow.
- [ ] Human validation is identified when automated checks are insufficient.
- [ ] No unresolved architecture decision is being delegated accidentally.

## Medium-risk additions

- [ ] Important interfaces and compatibility constraints are clear.
- [ ] Realistic failure behavior is covered.
- [ ] Relevant governing documents are linked.

## High-risk additions

When the work affects persistence, transactions, concurrency, recovery, idempotency, economy, security, protocol compatibility, authority, or architecture:

- [ ] Applicable invariants are explicit.
- [ ] Retry, failure, and recovery semantics are defined.
- [ ] Compatibility or migration behavior is defined.
- [ ] Resource or security limits are defined where needed.
- [ ] An ADR exists for high-cost or irreversible decisions.

Expected-file lists, exhaustive non-goals, evidence plans, stop conditions, and universal risk sections are optional. Include them only when they materially reduce ambiguity or risk.
