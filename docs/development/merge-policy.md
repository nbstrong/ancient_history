# Merge Policy

Implementation pull requests use squash merge and require:

- A linked implementation issue.
- Passing required automated checks.
- Browser review approval.
- Resolution of all blocking review comments.
- Human Godot validation for engine-affecting changes.
- Human evidence tied to the exact proposed head commit.
- No unapproved architecture or scope deviation.

Repository administrators should configure branch protection for the default branch after CI check names exist. Direct pushes to the default branch should be disabled for ordinary implementation work.
