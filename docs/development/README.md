# Development Process

Start here:

- [Development process principles](process-principles.md)
- [Risk-based development workflow](workflow.md)

## Pull requests and merge

- [Pull request lifecycle](pr-lifecycle.md)
- [Merge policy](merge-policy.md)
- [Pull request review checklist](review-checklist.md)
- [Browser review prompt](agent-review-prompt.md)

## Issues and agents

- [Issue authoring standard](issue-authoring.md)
- [Specification-ready checklist](specification-ready-checklist.md)
- [Implementation agent execution rules](agent-execution.md)
- [Label taxonomy](status-labels.md)

## Validation

- [Automated evidence](test-evidence.md)
- [Minimal editor check](human-test-report.md)

The implementation agent owns all command-line validation, including PowerShell and Windows tools invoked from WSL. Human involvement is limited to opening Godot and checking affected visible or interactive behavior. No proof is required; merging is the signoff.

Use the lightest process that safely fits the change. Stronger specification and automated testing apply only to risks actually affected.