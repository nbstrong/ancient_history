# Development Toolchain

The repository uses one pinned development toolchain:

- .NET SDK `10.0.302`, selected by the root `global.json`.
- Godot `4.7.1-stable` .NET double-precision editor. Accepted complete forms
  are `4.7.1.stable.mono.double.official`,
  `4.7.1.stable.mono.double.official.<hash>`,
  `4.7.1.stable.mono.double.custom_build`, and
  `4.7.1.stable.mono.double.custom_build.<hash>`, where `<hash>` is nonempty
  and ASCII alphanumeric (`A-Z`, `a-z`, or `0-9`).
- Godot project features: `4.7`, `Double Precision`, and `Forward Plus`.

## Supported development environments

Development is supported on x64 Windows with PowerShell and on x64 Linux with
Bash. WSL or Git Bash may be used to run the Bash launcher against a Windows
Godot executable. Mobile and web development are outside this toolchain's
scope.

Install the exact .NET SDK and Godot .NET editor through the normal operating
system or vendor installation process. The repository does not download,
install, or redistribute either tool.

## Selecting Godot

`GODOT_BIN` is the authoritative executable override. Set it to the installed
Godot .NET editor using the path syntax of the shell:

```bash
export GODOT_BIN=/path/to/Godot
./open_godot.sh
```

```powershell
$env:GODOT_BIN = 'C:\Path\To\Godot.exe'
.\open_godot.ps1
```

When `GODOT_BIN` is not set, both launchers search PATH in this order:
`godot4-mono`, `godot-mono`, `godot4`, `godot`, `Godot.exe`, and `godot.exe`.
The selected executable must report the supported release and pass the
headless .NET import/build check.

## Verification

From the repository root, verify the pinned SDK and Godot editor before opening
the project:

```bash
dotnet --version
bash tools/verify-toolchain.sh
```

On Windows PowerShell:

```powershell
dotnet --version
.\tools\verify-toolchain.ps1
```

Verification requires `dotnet --version` to print `10.0.302`, Godot to report
the normalized release `4.7.1.stable.mono.double.<provenance>`, expose the .NET-only
`--build-solutions` help option (including when terminal ANSI styling surrounds
the option), and complete:

```text
--headless --editor --path <project> --build-solutions --quit
```

The launcher also performs the version check and returns a nonzero exit code
before opening the project when the executable is missing, unsupported, or a
preview release.

## Upgrade policy

Toolchain upgrades are issue-driven. An upgrade must update `global.json`,
`tools/godot-version.txt`, both launcher implementations, verification tests,
and this document together. The new versions must be stable, available through
the supported installation process, and validated before the change is marked
ready. Do not silently roll forward to a different major, minor, preview, or
SDK version.
