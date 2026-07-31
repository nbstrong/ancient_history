# Browser Review Prompt

Use the following prompt when requesting a browser-based review of an implementation pull request:

> Review this pull request against its linked issue, accepted ADRs, governing feature specifications, and repository invariants. Verify every acceptance criterion and requirement-mapping entry. Identify missing tests, unrelated scope, architectural deviations, concurrency or recovery failures, insecure client trust, protocol incompatibilities, migration risks, and undocumented behavior. Do not approve based only on passing CI. Distinguish blocking defects from optional follow-up improvements. Evaluate whether the human Godot validation procedure is sufficient and whether any reported human result applies to the exact head commit. End with one disposition: Approve, Comment, or Request changes.

Use the output format in `docs/development/review-checklist.md`.
