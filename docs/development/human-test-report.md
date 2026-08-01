# Editor Check

There is no human test report workflow.

The implementation agent owns all command-line validation, including PowerShell and Windows tooling invoked from WSL, plus headless Godot checks.

A human is needed only when visible or interactive Godot behavior must be inspected in the editor.

Before merging such a pull request:

1. Open the project in Godot.
2. Look at or interact with the affected behavior.
3. Merge only when it appears correct.

No screenshot, video, written report, environment record, commit SHA, checklist, or PR comment is required. The merge is the record that the editor check was accepted.