# Pull Request Lifecycle

## 1. Start

- Choose an observable objective.
- Classify the affected risk.
- Use an issue when dependencies, invariants, milestone ordering, or coordination benefit from one.
- Create a descriptive branch.

Draft status is optional.

## 2. Implement

- Keep the change focused on the objective.
- Make ordinary implementation decisions from repository conventions.
- Add tests proportional to affected behavior and risk.
- Stop only for unresolved architecture, incompatible dependencies, public-contract changes, or significant scope expansion.

## 3. Agent validation

The implementation agent runs every practical non-visual check:

- .NET builds and tests.
- Bash scripts.
- PowerShell scripts invoked through `powershell.exe` or `pwsh` from WSL.
- Windows executables invoked from WSL when needed.
- Headless Godot import/build/runtime checks.
- Server, database, networking, persistence, recovery, and compatibility tests.

Record concise PASS, FAIL, and genuinely unavailable results. Do not delegate terminal commands to a human.

## 4. Review

- Review the supported workflow and affected invariants first.
- Block only on material correctness, durability, compatibility, authority, security, architecture, or required-test failures.
- Record nonessential hardening as follow-up work.
- After fixes, verify the fixes and affected areas rather than restarting the entire review without new risk.

## 5. Editor check

When visible or interactive Godot behavior changed, the merger opens the editor and inspects the affected behavior.

The pull request should say what to open and what to look for. No report, screenshot, video, copied output, environment record, tested SHA, or validation comment is required.

Merging is the signoff that the editor check passed.

## 6. Merge

Merge when:

- The objective works.
- Required automated checks pass.
- No material blocker remains.
- The merger is satisfied with any required editor check.
- Architecture changes are approved.

Use squash merge by default.

## 7. Closeout

- Confirm linked issues close when applicable.
- Unblock dependent work.
- Create follow-up issues only for improvements worth scheduling.

Do not delay completion to exhaustively harden unsupported edge cases or produce redundant evidence.