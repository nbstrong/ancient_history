# Browser Review Prompt

Use this prompt for implementation pull requests:

> Review this pull request against its observable objective, stated risk level, relevant issue or ADR, and the repository's non-negotiable architecture boundaries. Start with the supported workflow and material invariants. Identify concrete blockers: missing required behavior, realistic supported-workflow failures, data loss or duplication, recovery failure, security or authority violations, incompatible contracts, unapproved architecture, or missing tests for a material affected risk. Treat cleanup, speculative hardening, unsupported edge cases, and broader parity improvements as follow-ups unless explicitly required. Apply persistence, concurrency, recovery, protocol, security, and Godot checks only when the change affects them. After prior fixes, verify the fixes and affected areas rather than restarting the entire review without new risk. End with Pass or Changes required.

Use the concise output in `docs/development/review-checklist.md`.
