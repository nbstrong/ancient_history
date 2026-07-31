# Risk-Based Development Workflow

## Purpose

This repository uses delegated coding agents and human review to ship working vertical slices quickly without weakening critical invariants. Process is proportional to risk. See [Development Process Principles](process-principles.md).

## Start With Reality

Before specifying exact external-tool output, file formats, commands, or platform behavior, inspect the real supported environment when practical. Observed behavior is the starting evidence. Do not build a contract around guessed tool output.

## Risk Classification

Classify each change before deciding its process:

- **Low risk:** documentation, local tooling, project metadata, scaffolding, mechanical refactors, and test-only changes.
- **Medium risk:** ordinary gameplay, client, server, networking, or persistence features without new architecture or critical state invariants.
- **High risk:** migrations, transactions, concurrency, recovery, idempotency, economy integrity, security, protocol compatibility, authoritative world state, or irreversible architecture.

When uncertain, choose the higher level only for the affected concern—not for the entire repository.

## Planning and Issues

### Low risk

A concise pull-request description may be sufficient. A separate issue is optional unless the work is part of milestone sequencing or has dependencies.

State:

- The observable result.
- Important constraints.
- Focused validation.

### Medium risk

Use an issue with objective, dependencies, relevant constraints, acceptance criteria, and human validation when applicable.

### High risk

Use an issue with explicit invariants, failure semantics, compatibility/recovery expectations, and an ADR when a high-cost or irreversible decision is involved.

Expected-file lists, exhaustive non-goals, stop conditions, and evidence plans are optional unless they materially reduce ambiguity or risk.

## Branches and Pull Requests

Use a descriptive branch name. Issue branches may use `issue-<number>-<short-description>`.

A pull request should:

- Link an issue when one exists.
- Explain what changed and why.
- State the risk level.
- Report focused validation with PASS/FAIL summaries.
- Identify human testing still needed.

Draft status is useful for incomplete or high-risk work but is not mandatory for a complete low-risk change.

Use squash merge unless preserving separate commits has clear value.

## Implementation Agent Rules

The implementation agent must:

1. Read the relevant issue and governing documents.
2. Inspect real tools or interfaces before encoding exact assumptions when practical.
3. Stay within the stated objective.
4. Add tests proportional to the behavior and risk.
5. Report validation accurately.
6. Stop when an unresolved architecture decision, incompatible dependency, or material scope expansion is required.

The agent may make ordinary implementation choices that do not change architecture or public contracts. It should not stop for minor unspecified details that can be resolved safely from repository conventions.

Do not bundle unrelated work, but allow small cleanup that is necessary to complete or test the objective.

## Review Rules

Reviewers first ask whether the supported workflow works and required invariants hold.

A **blocking defect** is a realistic material problem such as:

- A missing required outcome.
- A supported workflow that fails.
- Data loss, corruption, duplication, or recovery failure.
- A security or authority violation.
- An incompatible public or serialized contract.
- Unapproved architecture or dangerous scope expansion.
- A required test that is absent for a material risk.

Everything else is a follow-up improvement unless the issue explicitly makes it required.

Reviewers should:

- Prioritize concrete failures over hypothetical edge cases.
- Verify requested fixes without restarting the entire review unless new risk was introduced.
- Avoid demanding mathematical equivalence across platforms when supported behavior is equivalent.
- Amend a defective specification once using observed evidence rather than redesigning through repeated comments.
- Stop expanding test matrices once the supported workflow and stated invariants are adequately proven.

## Validation and Evidence

Evidence is proportional:

- Prefer commands plus concise PASS/FAIL summaries.
- Include full logs only for failures, performance claims, recovery behavior, or high-risk invariants.
- Do not require exhaustive requirement-to-file mapping for low-risk work.
- Record unavailable validation honestly without blocking unrelated validated behavior.

## Human Godot Testing

Human engine testing is required when automated checks cannot adequately prove affected visible, interactive, import, export, graphics, input, audio, connection, or runtime behavior.

Use a focused procedure covering only affected behavior. Screenshots or video are required only when they prove a visual or interactive acceptance criterion.

Human evidence remains valid after later commits that cannot affect the tested behavior. Documentation-only, test-only, and unrelated metadata changes do not automatically require repeating editor validation.

## Merge Rules

Merge when:

- The objective is met.
- Required automated checks for the affected behavior pass.
- No material blocker remains.
- Required human validation passes.
- Any unapproved architecture change has been removed or recorded in an ADR.

A linked issue, draft phase, exact-head screenshot, exhaustive risk review, and full test transcript are not universal merge requirements.

## Delivery Bias

Prefer a working vertical slice and follow-up issues over indefinitely hardening a small change. Reserve exhaustive review for failures that could corrupt persistent state, violate authority, break compatibility, lose player data, or create serious security risk.
