# Test Evidence Requirements

## Automated Evidence

Every implementation pull request records:

- Exact validation commands.
- Exit results.
- Relevant test names.
- CI run link when available.
- Failure-injection or concurrency-test configuration when applicable.

Do not use screenshots of terminal output when copyable text or CI logs are available.

## Human Evidence

Engine-affecting pull requests include:

- Tested commit SHA.
- Godot version and operating system.
- Exact procedure.
- Pass or fail result.
- Screenshots for visible static state.
- Video for interaction, timing, reconnect, animation, or multi-client behavior.
- Logs for state revisions, server decisions, and recovery.

Evidence must demonstrate the issue acceptance criteria rather than only showing that the project launched.
