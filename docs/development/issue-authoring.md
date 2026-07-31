# Issue Authoring Standard

## Purpose

Issues should make implementation easier, not front-load every possible review concern. Use the minimum detail needed for the change's risk level.

## Low-Risk Work

A separate issue is optional for documentation, local tooling, project metadata, scaffolding, mechanical refactors, and test-only fixes.

When an issue is useful, include only:

1. Objective
2. Relevant context or constraints
3. Acceptance criteria
4. Human validation, when applicable

Do not require expected-file lists, exhaustive non-goals, evidence plans, stop conditions, or a custom definition of done unless they prevent a real ambiguity.

## Medium-Risk Work

Include:

1. Objective
2. Dependencies
3. Relevant governing documents
4. Required behavior and important interfaces
5. Acceptance criteria for success and realistic failure paths
6. Human validation when automated checks are insufficient

Leave implementation details to the agent unless they are compatibility, architecture, or safety constraints.

## High-Risk Work

For migrations, transactions, concurrency, recovery, idempotency, economy integrity, security, protocol compatibility, authoritative state, or irreversible architecture, also define the applicable:

- Invariants
- Failure and retry semantics
- Recovery behavior
- Compatibility expectations
- Resource limits
- ADR decisions

Only high-risk concerns need adversarial or failure-injection acceptance criteria by default.

## Observe Before Specifying

Inspect real supported tools and interfaces before writing exact parsers, output formats, command behavior, or platform assumptions when practical. If observed behavior differs from the issue, correct the issue once and continue from the observed contract.

## Acceptance Criteria

Each criterion should prove an observable result. Prefer a small set of meaningful checks over exhaustive matrices.

Good:

- `dotnet build` succeeds.
- The supported Godot editor completes a headless import/build.
- Retrying a committed command does not duplicate state.

Avoid criteria that exist only to exercise remote or unsupported edge cases.

## Human Validation

Require human testing only for affected behavior that automated checks do not adequately prove. State the action and expected observation. Request screenshots or video only when they are the clearest evidence of a visual or interactive result.

## Stop Conditions

Use stop conditions only for material uncertainty, such as:

- A governing ADR conflicts with the work.
- A new architecture or runtime dependency is required.
- A public contract must change unexpectedly.
- The task cannot meet its objective without significant scope expansion.

Agents should resolve minor unspecified implementation details through repository conventions rather than stopping.

## Sizing

Prefer one observable result per pull request. Split work when concerns are independently useful, separately risky, or require unrelated validation—not merely because several files or layers are touched.
