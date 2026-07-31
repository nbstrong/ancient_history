# Development Process Principles

This repository optimizes for fast, reliable vertical-slice delivery. Process exists to reduce risk, not to maximize paperwork.

## Default posture

- Observe real tools and interfaces before writing exact specifications.
- Use the lightest process that safely fits the change.
- Block only on defects that can materially break a supported workflow, violate a required invariant, lose data, create an incompatible contract, or introduce unapproved architecture.
- Record useful evidence, not exhaustive transcripts.
- Turn nonessential hardening into follow-up work instead of extending the current review indefinitely.

## Risk levels

### Low risk

Examples: documentation, local tooling, project metadata, simple scaffolding, mechanical refactors, and test-only changes.

Expected process:

- A concise issue or clearly scoped PR description.
- Focused validation of the changed behavior.
- One reviewer pass plus verification of requested fixes.
- Human testing only when the change affects visible or interactive behavior.

### Medium risk

Examples: ordinary gameplay, client, server, networking, or persistence features without new architecture or critical invariants.

Expected process:

- Observable acceptance criteria.
- Automated tests for success and realistic failure paths.
- Human testing for affected interactive behavior.
- Review focused on correctness, maintainability, and compatibility.

### High risk

Examples: migrations, transactions, concurrency, recovery, idempotency, economy integrity, authentication, security boundaries, protocol compatibility, authoritative world-state rules, and irreversible architecture decisions.

Expected process:

- Explicit invariants and failure semantics.
- Adversarial tests where justified.
- Detailed review and evidence for the affected risk.
- ADRs for high-cost or irreversible decisions.

## Evidence policy

Evidence must be proportional to the change:

- Prefer a short list of commands with PASS/FAIL summaries.
- Include detailed logs only for failures, performance claims, recovery behavior, or high-risk invariants.
- Screenshots and video are required only when they prove visible or interactive acceptance criteria.
- A later commit invalidates human evidence only when it can affect the tested behavior.
- Test-only, documentation-only, and unrelated metadata changes do not automatically require repeating prior human validation.

## Review policy

A reviewer should:

1. Check the linked objective and affected risk.
2. Identify material blockers first.
3. Classify nonessential improvements as follow-ups.
4. Verify requested fixes without restarting the entire review from scratch unless the change creates new risk.
5. Stop expanding edge-case coverage once the supported workflow and stated invariants are adequately proven.

Review comments should not silently redesign a correct issue. When the specification is wrong, amend it once using observed evidence and continue from the corrected contract.
