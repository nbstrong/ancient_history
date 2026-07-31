# Test Evidence Requirements

Evidence should make the result trustworthy without turning the pull request into a transcript archive.

## Automated Evidence

For low- and medium-risk changes, record:

- The focused commands or checks performed.
- A concise PASS/FAIL result.
- Any unavailable validation that matters.

Include detailed output only when it explains a failure, performance claim, recovery result, or high-risk invariant.

For high-risk persistence, concurrency, recovery, protocol, security, authority, or economy work, also record the configuration and evidence needed to understand the affected invariant.

CI links are useful when available but are not additional paperwork requirements when the result is already clear.

## Human Evidence

Record:

- The supported environment.
- The affected behavior tested.
- Pass or fail.
- Relevant observations or defects.

Include the commit or branch state when behavior-sensitive changes are still occurring. A later commit invalidates human evidence only when it can affect the tested behavior.

Use screenshots for visible static results and video for interaction or timing only when those artifacts prove an acceptance criterion. A successful command result may be better evidence for toolchain, import, build, or headless behavior.

Do not require terminal screenshots when copyable output is available.
