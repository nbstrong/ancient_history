# Delegated Development Workflow

## Purpose

This repository is designed for issue-driven implementation by delegated coding agents, followed by automated validation, browser-based review, and human testing in Godot when engine behavior is affected.

An issue is an executable specification. An implementation agent applies decisions recorded in the issue, specifications, and accepted ADRs. It does not invent architecture during implementation.

## Delivery Gates

Every implementation passes three independent gates.

### 1. Specification ready

An issue may receive work only when:

- Its objective is observable and limited to one primary outcome.
- Dependencies are merged or explicitly available.
- Required interfaces and constraints are specified.
- Automated acceptance criteria are objectively testable.
- Human validation is either specified or explicitly not required.
- Stop conditions identify when the implementer must request clarification.

### 2. Engineering ready

A pull request may be approved only when:

- The linked issue is fully implemented.
- The diff contains no unrelated work.
- Required tests and failure cases pass.
- Public interfaces and serialized contracts match governing documents.
- Reviewer comments are resolved.
- Human-test instructions are complete.

### 3. Product verified

An engine-affecting change may be merged only when a human has tested the exact proposed commit and recorded the result. A later code change invalidates affected human-test evidence.

## Issue States

Use these labels when available:

- `status:planned`: ordered but not executable.
- `status:ready`: all dependencies and specification requirements are satisfied.
- `status:in-progress`: one implementation agent owns the task.
- `status:review`: implementation and automated checks are complete.
- `status:human-test`: code review passed and engine validation remains.
- `status:blocked`: a named dependency or decision prevents progress.

Suggested area labels:

- `area:client`
- `area:server`
- `area:protocol`
- `area:persistence`
- `area:networking`
- `area:terrain`
- `area:infrastructure`
- `area:testing`

Suggested risk labels:

- `risk:low`: mechanical change with narrow failure impact.
- `risk:medium`: ordinary subsystem implementation.
- `risk:high`: persistence, concurrency, recovery, protocol, security, or architecture-sensitive work.

Suggested agent-routing labels:

- `agent:small`: mechanical and tightly constrained.
- `agent:standard`: ordinary implementation with complete interfaces.
- `agent:strong`: concurrency, persistence, protocol, or cross-boundary work.

## Issue Naming and Ordering

Implementation issue titles use:

```text
[M<milestone>-<sequence>] Imperative objective
```

Example:

```text
[M0-003] Create standalone executable world server
```

Every issue lists both `Blocked by` and `Blocks`. Only issues with merged dependencies receive `status:ready`.

Issue numbers do not define architecture order. The milestone sequence and explicit dependencies do.

## Issue Size

A normal issue should have:

- One observable result.
- One pull request.
- One primary subsystem.
- Approximately one to five expected implementation files, excluding tests and generated metadata.
- Tests in the same pull request.
- No unresolved architecture decision.

Split work when an issue combines independent concerns such as:

- Schema design and networking.
- Server behavior and Godot rendering.
- Terrain math and persistence.
- Outbox storage and publisher recovery.
- Multiple gameplay actions.

## Branch and Pull Request Rules

Branch name:

```text
issue-<number>-<short-description>
```

Pull requests must:

- Link one implementation issue with `Closes #<number>`.
- Open as draft while implementation is incomplete.
- Use the repository pull-request template.
- Map requirements to code and evidence.
- Explain all deviations and files outside the issue's expected-file list.
- Mark ready only after automated criteria pass.

Use squash merging so each issue becomes one coherent commit on the default branch.

## Implementation Agent Rules

The implementation agent must:

1. Work only from an issue marked ready.
2. Read every governing specification and ADR named in the issue.
3. Implement only the required scope.
4. Add or update required tests.
5. Preserve client/server authority boundaries.
6. Record exact validation commands and results.
7. Stop rather than improvise when a stop condition is met.

The implementation agent must not:

- Introduce a new architecture pattern without an accepted ADR.
- Add a dependency that the issue does not permit.
- Weaken an acceptance criterion to make a test pass.
- Claim human validation.
- Bundle cleanup or unrelated refactors into the task.

## Review Rules

The reviewer evaluates the pull request against:

1. The linked issue.
2. Accepted ADRs.
3. Governing feature specifications.
4. Repository invariants.
5. Automated and human evidence requirements.

The reviewer distinguishes:

- **Blocking defect:** correctness, missing requirement, unsafe authority boundary, durability failure, missing required test, incompatible contract, or unapproved scope expansion.
- **Follow-up improvement:** valuable work not required for safe completion of the issue.

A defective issue specification should be amended or replaced. The reviewer should not silently redesign the subsystem through ad hoc review comments.

## Human Godot Testing

Human engine testing is required for changes affecting:

- Godot startup or project import.
- Scenes, nodes, resources, meshes, shaders, animation, input, camera, audio, or UI.
- Client connection and reconnect behavior.
- Player interaction and visible world state.
- Client-side application of snapshots or deltas.
- Export or runtime behavior in supported environments.

Backend-only tasks may state that human engine testing is not required, but the issue and PR must explain why.

Use `docs/development/human-test-report.md`. The report must include the tested commit SHA, Godot version, operating system, configuration, procedure, result, and evidence.

## Merge Rules

A pull request is mergeable only when:

- All required automated checks pass. Before CI exists, the pull request must record successful issue-specific validation.
- Required review is approved.
- All blocking comments are resolved.
- Human validation passes when required.
- The tested commit matches the proposed head commit.
- The branch is current enough to merge without invalidating test assumptions.

## Planning Depth

Maintain planning at these levels:

- Current milestone: fully specified executable issues.
- Next milestone: fully specified before current milestone completion.
- Later milestones: ordered issue shells or epics, not ready for implementation.

This avoids implementing distant features against interfaces that have not yet been proven by the first end-to-end vertical slice.
