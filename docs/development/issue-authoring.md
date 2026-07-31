# Issue Authoring Standard

## Purpose

Implementation issues are executable contracts for delegated agents. They must remove avoidable interpretation while preserving a narrow place for implementation judgment.

## Required Sections

Every implementation issue contains:

1. Objective
2. Background and governing documents
3. Dependencies
4. Required changes
5. Expected files
6. Required interfaces and data contracts
7. Implementation constraints
8. Non-goals
9. Automated acceptance criteria
10. Human engine validation
11. Evidence required
12. Stop conditions
13. Definition of done

Use `.github/ISSUE_TEMPLATE/implementation.yml` to enforce these sections.

## Writing Requirements

### Objective

Describe one observable outcome. Avoid broad subsystem names.

Good:

> Add an executable world-server host that reports readiness and shuts down gracefully.

Bad:

> Implement the backend.

### Required Changes

Specify exact behavior and boundaries. Name files, public types, commands, serialized fields, schema constraints, scene nodes, and error behavior when those details are known.

Do not ask the implementation agent to choose among unresolved architectures. Record the choice in an ADR first.

### Automated Acceptance Criteria

Each criterion must produce a pass or fail result. Prefer named tests and commands.

Good:

- `SameKeyDifferentHash_ReturnsConflictWithoutCallingDomainMutation`
- `dotnet build -warnaserror` exits successfully.

Bad:

- Code is robust.
- Networking works well.

### Human Validation

Provide exact prerequisites, actions, observations, and regression checks. Visual or interactive acceptance criteria must not be replaced by a general request to "test in Godot."

### Stop Conditions

Lower-capability agents must stop when:

- An accepted ADR conflicts with the issue.
- A dependency is missing or has a different interface.
- A new dependency or architecture decision appears necessary.
- Required behavior cannot be implemented without expanding scope.
- A test requirement appears impossible or internally inconsistent.

## Issue Sizing

Split an issue when it includes more than one independently reviewable result, crosses unrelated subsystems, or requires separate human validation procedures.

The expected default is one issue, one branch, one pull request, and one squash commit.

## Ready Review

Before applying `status:ready`, a reviewer confirms:

- Dependencies are merged.
- Governing documents exist on the default branch.
- Interfaces are exact enough to implement.
- Tests cover success and relevant failure behavior.
- Human validation is actionable.
- No architecture decision remains open.
