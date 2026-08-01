# Implementation Agent Execution Rules

1. Read the relevant issue and governing documents.
2. Inspect real supported tools before encoding exact assumptions.
3. Keep the change focused on the observable objective.
4. Preserve architecture, authority, durability, and compatibility boundaries.
5. Make ordinary implementation choices from repository conventions.
6. Add tests proportional to the affected behavior and risk.
7. Run all practical command-line validation yourself.
8. Use `powershell.exe`, `pwsh`, or Windows executables from WSL when Windows-specific checks are needed.
9. Run headless Godot checks yourself when supported by the environment.
10. Report concise PASS, FAIL, and genuinely unavailable results.
11. Stop only for unresolved architecture, incompatible dependencies, unexpected public-contract changes, or significant scope expansion.
12. Leave the repository buildable and reviewable.

Do not delegate terminal commands to a human merely because they use PowerShell or Windows tooling.

The human role is limited to opening Godot and checking affected visible or interactive behavior. The agent should state what the merger needs to look at, but must not request screenshots, reports, SHA attestations, or copied command output.