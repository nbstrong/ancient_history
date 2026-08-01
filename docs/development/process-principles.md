# Development Process Principles

This repository optimizes for fast, reliable vertical-slice delivery. Process exists to reduce risk, not create handoffs.

## Default posture

- Agents own implementation and all command-line validation.
- WSL is the normal agent environment. Agents may invoke Windows tools, including `powershell.exe` or `pwsh`, when platform-specific checks are needed.
- Humans are not command runners or evidence collectors.
- Human involvement is limited to opening Godot and checking visible or interactive behavior that cannot be validated automatically.
- Merging an editor-affecting pull request means the merger performed and accepted that editor check.
- No screenshot, video, report, tested-SHA attestation, or separate validation comment is required.
- Block only on defects that materially break a supported workflow, violate a required invariant, lose data, create an incompatible contract, or introduce unapproved architecture.
- Turn nonessential hardening into follow-up work instead of extending the current review.

## Risk levels

### Low risk

Documentation, local tooling, project metadata, simple scaffolding, mechanical refactors, and test-only changes.

Use a concise description, focused automated validation, and no editor check unless visible Godot behavior changed.

### Medium risk

Ordinary gameplay, client, server, networking, or persistence features without new architecture or critical state invariants.

Use observable acceptance criteria and automated tests for success and realistic failures. Add an editor check only for affected visible or interactive behavior.

### High risk

Migrations, transactions, concurrency, recovery, idempotency, economy integrity, security boundaries, protocol compatibility, authoritative world-state rules, and irreversible architecture decisions.

Use explicit invariants, deeper automated tests, and ADRs where appropriate. Human involvement is still limited to the editor check.

## Validation policy

Agents should run every practical check themselves, including:

- .NET builds and tests.
- Bash tooling.
- Windows PowerShell scripts invoked from WSL when needed.
- Headless Godot import, build, and runtime checks.
- Database, server, networking, and recovery tests.

Record concise PASS/FAIL summaries. Include detailed logs only when they explain a failure or high-risk result.

## Editor check

When a change affects visible or interactive Godot behavior, the merger opens the editor and checks the affected behavior before merging. This is an informal product sanity check, not a separate evidence workflow.

No report is required. No artifact is required. Merge is the signoff.

## Review policy

A reviewer should:

1. Check the objective and affected risk.
2. Identify material blockers first.
3. Treat nonessential improvements as follow-ups.
4. Verify requested fixes without restarting the entire review unless new risk was introduced.
5. Stop expanding edge-case coverage once the supported workflow and stated invariants are adequately proven.

When a specification is wrong, correct it once using observed behavior and continue.