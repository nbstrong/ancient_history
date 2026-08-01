# Risk-Based Development Workflow

## Purpose

Ship working vertical slices quickly while preserving critical invariants. Process is proportional to risk and should avoid human handoffs.

## Start with reality

Inspect real tools, interfaces, and supported environments before specifying exact output or behavior. Do not build contracts around guessed tool output.

## Risk classification

- **Low:** documentation, local tooling, metadata, scaffolding, mechanical refactors, and test-only changes.
- **Medium:** ordinary gameplay, client, server, networking, or persistence features.
- **High:** migrations, transactions, concurrency, recovery, idempotency, economy integrity, security, protocol compatibility, authoritative state, or irreversible architecture.

Apply additional rigor only to the risk actually affected.

## Planning

Low-risk work may use a concise pull request without a separate issue. Medium- and high-risk work should use an issue when dependencies, invariants, coordination, or milestone ordering benefit from one.

State the observable result, material constraints, and focused acceptance criteria. Expected-file lists, exhaustive non-goals, evidence plans, and custom definitions of done are optional.

## Agent execution

The implementation agent owns the complete non-visual workflow:

1. Implement the objective.
2. Run focused automated checks.
3. Run broader checks when shared surfaces or risk justify them.
4. Run platform-specific command-line checks itself.
5. Report concise PASS, FAIL, and genuinely unavailable results.

WSL is a supported agent environment. When Windows-specific validation is needed, the agent should invoke `powershell.exe`, `pwsh`, Windows executables, or other available host tools from WSL rather than delegating command execution to a human.

The agent may make ordinary implementation decisions from repository conventions. It should stop only for unresolved architecture, incompatible dependencies, unexpected public-contract changes, or significant scope expansion.

## Pull requests

A pull request should explain:

- What changed.
- Why it changed.
- Focused automated validation.
- Whether an editor check is needed and what the merger should look at.

Draft status, requirement-to-file mapping, full transcripts, screenshots, and exact-head attestations are not default requirements.

## Review

A blocking defect is a realistic material problem:

- Missing required behavior.
- Broken supported workflow.
- Data loss, corruption, duplication, or recovery failure.
- Security or authority violation.
- Incompatible public or serialized contract.
- Unapproved architecture or dangerous scope expansion.
- Missing automated coverage for a material affected risk.

Cleanup, speculative hardening, unsupported edge cases, and broad parity improvements are follow-ups unless explicitly required.

After fixes, verify the requested fixes and affected areas. Do not restart the entire review without new risk.

## Automated validation

Agents run all practical command-line validation, including:

- .NET builds and tests.
- Bash scripts.
- PowerShell scripts through `powershell.exe` or `pwsh` from WSL when needed.
- Headless Godot checks.
- Server, database, networking, persistence, recovery, and compatibility tests.

Record short PASS/FAIL summaries. Detailed logs are needed only to explain failures or high-risk results.

## Editor check

Human involvement is limited to opening Godot and checking visible or interactive behavior that automation cannot adequately establish.

The issue or pull request should say only what to open and what to look for. No formal procedure, report, screenshot, video, environment record, commit SHA, or validation comment is required.

Merging the pull request is the human signoff that any required editor check was completed and acceptable.

## Merge

Merge when:

- The objective works.
- Required automated checks pass.
- No material blocker remains.
- The merger is satisfied with any required editor check.
- Architecture changes are intentional and approved.

Prefer a working vertical slice and follow-up issues over indefinitely hardening a small change.