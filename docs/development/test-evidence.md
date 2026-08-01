# Test Evidence Requirements

Evidence should make automated results understandable without turning a pull request into a transcript archive.

## Agent-owned validation

The implementation agent runs all practical command-line checks, including:

- .NET builds and tests.
- Bash tooling.
- PowerShell scripts through `powershell.exe` or `pwsh` from WSL.
- Windows executables from WSL when needed.
- Headless Godot checks.
- Server, database, networking, persistence, recovery, and compatibility tests.

Record focused commands or checks with concise PASS, FAIL, or genuinely unavailable results.

Include detailed output only when it explains a failure, performance claim, recovery result, or high-risk invariant.

## Editor check

No human evidence package is required.

When visible or interactive Godot behavior changed, the merger opens the editor and checks that behavior before merging. No screenshot, video, report, environment record, tested SHA, copied output, or validation comment is required.

The merge itself is the signoff.