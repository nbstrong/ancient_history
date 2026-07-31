# Merge Policy

A pull request may merge when:

- Its observable objective is complete.
- Required automated checks for the affected behavior and risk pass.
- No material blocking review comment remains.
- Required human validation passes.
- Public contracts and architecture changes are intentional and approved.
- The branch is mergeable.

A linked issue is recommended for medium/high-risk work and coordinated milestone work, but is optional for low-risk documentation, tooling, metadata, scaffolding, mechanical, and test-only changes.

Draft status, exhaustive requirement mapping, universal risk checklists, full command transcripts, screenshots, and exact-head retesting are not universal requirements.

Human evidence remains valid after later commits that cannot affect the tested behavior.

Use squash merge by default. Direct pushes to the default branch should remain uncommon for ordinary implementation work once branch protection and CI are configured.
