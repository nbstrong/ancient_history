# Merge Policy

A pull request may merge when:

- Its observable objective is complete.
- Required automated checks pass.
- No material blocking review comment remains.
- Public contracts and architecture changes are intentional and approved.
- The branch is mergeable.

The implementation agent owns all command-line validation, including Windows PowerShell and Windows executable checks performed from WSL when needed.

For changes that affect visible or interactive Godot behavior, the merger opens the editor and checks the affected behavior before merging. No report, screenshot, video, environment record, tested SHA, or separate validation comment is required. The merge itself is the signoff that the editor check was acceptable.

A linked issue is recommended for medium/high-risk work and coordinated milestone work, but is optional for low-risk documentation, tooling, metadata, scaffolding, mechanical, and test-only changes.

Draft status, exhaustive requirement mapping, universal risk checklists, full command transcripts, human evidence packages, and exact-head attestations are not merge requirements.

Use squash merge by default.