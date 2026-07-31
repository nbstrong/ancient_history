# Human Engine Test Report

Copy this template into the implementing pull request or attach a completed report. Test the exact proposed head commit.

## Identification

- Pull request:
- Linked issue:
- Commit SHA:
- Tester:
- Test date:

## Environment

- Godot version:
- .NET version:
- Operating system:
- GPU and graphics API, when relevant:
- Server configuration:
- Database state or seed:
- Client count:
- Export build or editor run:

## Prerequisites

1.
2.
3.

## Procedure Performed

1.
2.
3.

## Expected Results

- [ ]
- [ ]
- [ ]

## Observed Results

Describe visible behavior, state revisions, logs, reconnect behavior, restart behavior, timing, and any deviations.

## Required Regression Checks

- [ ] Project imports without unexpected errors.
- [ ] Client starts and exits cleanly.
- [ ] Server starts and exits cleanly.
- [ ] Existing connection flow remains functional.
- [ ] No new recurring errors appear in client or server logs.
- [ ] Issue-specific regressions were checked.

Mark non-applicable checks and explain why.

## Recovery Checks

When applicable:

- [ ] Client reconnect converges to authoritative state.
- [ ] Server restart restores canonical state.
- [ ] Duplicate input does not duplicate an effect.
- [ ] Interrupted action does not partially commit.
- [ ] Revision or sequence mismatch triggers the specified recovery path.

## Evidence

Attach or link:

- Screenshots for visible state.
- Video for interaction, timing, animation, reconnect, or multi-client behavior.
- Client logs.
- Server logs.
- Database queries or state dumps when persistence is under test.

## Defects Found

- None, or link each defect issue.

## Final Result

- [ ] PASS
- [ ] FAIL

Comments:

## Retest Policy

Any later commit that can affect the tested behavior invalidates this result. Record a new test report against the new head commit before merge.
