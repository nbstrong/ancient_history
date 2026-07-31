# Pull Request Lifecycle

## 1. Start

- Select an issue whose dependencies are merged and whose specification is ready.
- Create branch `issue-<number>-<short-description>` from the current default branch.
- Open a draft pull request using the repository template.
- Link the issue with `Closes #<number>`.

## 2. Implement

- Limit changes to the issue scope.
- Add tests in the same pull request.
- Update documentation required by the issue.
- Record deviations immediately rather than hiding them in the final diff.
- Stop when an issue stop condition is met.

## 3. Automated Validation

- Run all issue-required commands.
- Run the repository CI-equivalent command when it exists.
- Record commands and results in the pull request.
- Do not mark ready while required checks fail or are absent.

## 4. Browser Review

- Mark the pull request ready.
- Review against the linked issue, accepted ADRs, feature specifications, and repository invariants.
- Resolve every blocking defect.
- Re-run affected tests after changes.

## 5. Human Engine Validation

When required:

- Test the exact pull-request head commit.
- Use the pinned Godot version and documented environment.
- Complete `docs/development/human-test-report.md`.
- File separate defect issues for failures.
- Retest after any later commit that can affect the tested behavior.

## 6. Merge

Merge only when:

- Automated checks pass.
- Browser review approves.
- Human validation passes when required.
- The issue definition of done is complete.
- The branch remains mergeable.

Use squash merge. The squash commit should identify the issue and the observable result.

## 7. Closeout

- Confirm the issue closed automatically.
- Update the milestone tracking issue.
- Unblock only the next issues whose dependencies are now complete.
- Preserve evidence links in the pull request and issue history.
