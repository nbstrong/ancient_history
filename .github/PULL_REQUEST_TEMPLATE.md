## Linked issue

Closes #

## Requirement mapping

Map each substantive change to a requirement or acceptance criterion in the linked issue.

| Issue requirement | Implementation | Evidence |
|---|---|---|
| | | |

## Implementation summary

Describe what changed and why. Keep this limited to the linked issue.

## Deviations and scope changes

State `None` or explain every deviation from the issue, every file changed outside the expected-file list, and every new dependency.

## Automated validation

List exact commands and results.

```text
command
result
```

- [ ] Build succeeds with no new warnings.
- [ ] Required unit tests pass.
- [ ] Required integration tests pass.
- [ ] Existing test suite passes.
- [ ] Failure and recovery cases required by the issue are tested.

## Human engine validation

Do not claim this section passed unless a human tested the exact head commit.

Required: Yes / No

Commit tested:
Godot version:
Operating system:
Tester:
Result: Pending / Pass / Fail / Not required

Procedure and evidence:

## Risk review

Address applicable risks explicitly:

- Concurrency and ordering
- Persistence, migration, and recovery
- Idempotency and duplication
- Client/server authority
- Protocol compatibility
- Performance and resource limits
- Security and malformed input
- Godot scene, resource, and rendering regressions

## Reviewer checklist

- [ ] The PR implements the linked issue without unapproved architecture changes.
- [ ] No unrelated changes are present.
- [ ] Public interfaces and serialized contracts match the issue and ADRs.
- [ ] Tests prove success, rejection, retry, concurrency, and recovery behavior where applicable.
- [ ] Client input is not treated as authoritative state.
- [ ] Persistence changes include migration and rollback/recovery consideration.
- [ ] Human-test instructions are complete and reproducible.
- [ ] Documentation is updated.
