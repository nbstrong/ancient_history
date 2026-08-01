# Issue Authoring Standard

## Purpose

Issues should make implementation easier, not front-load every possible review concern. Use the minimum detail needed for the change's risk level.

## Low-risk work

A separate issue is optional for documentation, local tooling, project metadata, scaffolding, mechanical refactors, and test-only fixes.

When an issue is useful, include only:

1. Objective
2. Relevant context or constraints
3. Automated acceptance criteria
4. Editor check, when visible or interactive Godot behavior is affected

Do not require expected-file lists, exhaustive non-goals, evidence plans, stop conditions, or a custom definition of done unless they prevent a real ambiguity.

## Medium-risk work

Include the objective, dependencies, relevant constraints, important interfaces, and automated criteria for success and realistic failure paths.

## High-risk work

For migrations, transactions, concurrency, recovery, idempotency, economy integrity, security, protocol compatibility, authoritative state, or irreversible architecture, also define applicable invariants, failure semantics, recovery behavior, compatibility expectations, resource limits, and ADR decisions.

## Validation ownership

All command-line acceptance criteria belong to the implementation agent. This includes Bash, .NET, PowerShell, Windows executables, and headless Godot checks.

WSL is a supported agent environment. When Windows-specific checks are needed, write them as automated criteria and expect the agent to invoke `powershell.exe`, `pwsh`, or the relevant Windows executable from WSL.

Do not assign terminal commands to a human.

## Observe before specifying

Inspect real supported tools and interfaces before writing exact parsers, output formats, command behavior, or platform assumptions. If observed behavior differs from the issue, correct the issue once and continue from the observed contract.

## Automated acceptance criteria

Use a small set of observable checks covering success and realistic material failures. Avoid matrices that exist only to exercise unsupported or remote edge cases.

## Editor check

Require an editor check only for affected visible or interactive behavior that automation cannot adequately establish.

State only what the merger should open and what to look for. Do not request screenshots, video, reports, environment details, tested SHAs, copied command output, or a separate validation comment.

Merging the pull request means the editor check was accepted.

## Stop conditions

Use stop conditions only for material uncertainty: conflicting architecture, a new runtime dependency, an unexpected public-contract change, or significant scope expansion.

## Sizing

Prefer one observable result per pull request. Split work when concerns are independently useful, separately risky, or require unrelated implementation—not merely because several files or layers are touched.