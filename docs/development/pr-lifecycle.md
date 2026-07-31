# Pull Request Lifecycle

## 1. Start

- Choose an observable objective.
- Classify the affected risk.
- Use an issue for medium/high-risk work, milestone dependencies, or when coordination benefits from one.
- Low-risk work may begin from a concise pull request.
- Create a descriptive branch.

Draft status is optional. Use it when implementation or validation is still incomplete.

## 2. Implement

- Keep the change focused on the objective.
- Make ordinary implementation decisions from repository conventions.
- Add tests proportional to the affected behavior and risk.
- Stop only for unresolved architecture, incompatible dependencies, public-contract changes, or significant scope expansion.

## 3. Validate

- Run focused checks for the changed behavior.
- Run broader tests when the risk or shared surface justifies them.
- Record concise PASS/FAIL results.
- Report unavailable validation honestly.

## 4. Review

- Review the supported workflow and affected invariants first.
- Block only on material correctness, durability, compatibility, authority, security, architecture, or required-test failures.
- Record nonessential hardening as follow-up work.
- After fixes, verify the requested fixes and affected areas rather than restarting the entire review without new risk.

## 5. Human Validation

When automated checks cannot adequately prove affected Godot or runtime behavior:

- Run a focused human procedure in the supported environment.
- Record the behavior and result.
- Attach screenshots or video only when they prove the acceptance criterion.
- Retest after later commits only when they can affect the tested behavior.

## 6. Merge

Merge when:

- The objective works.
- Required checks for the affected risk pass.
- No material blocker remains.
- Required human validation passes.
- Architecture changes are approved.

Use squash merge by default. Preserve separate commits only when that history has clear value.

## 7. Closeout

- Confirm linked issues close when applicable.
- Unblock dependent work.
- Create follow-up issues only for improvements worth scheduling.

Do not delay completion to exhaustively harden unsupported edge cases or produce redundant evidence.
