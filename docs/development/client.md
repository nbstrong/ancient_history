# Godot .NET Client

The client is a Godot 4.7.1 .NET project rooted at `project.godot`. Its startup
scene displays the application name, offline state, and available assembly
informational version. It starts without a world server.

## Build and headless validation

Use the pinned SDK and Godot .NET editor described in
[`toolchain.md`](toolchain.md). From the repository root on Linux or WSL:

```bash
dotnet build AncientHistory.Client.csproj -warnaserror
bash tools/verify-toolchain.sh
source tools/toolchain.sh
godot_bin="$(resolve_godot_bin)"
project_path="$(godot_project_path "${godot_bin}" "$(pwd)")"
"${godot_bin}" --headless --path "${project_path}" --quit-after 2
```

`verify-toolchain.sh` performs the clean headless import and C# solution build.
The final command starts the configured main scene and asks Godot to exit after
two main-loop iterations, keeping runtime validation bounded.

The equivalent PowerShell validation is:

```powershell
dotnet build AncientHistory.Client.csproj -warnaserror
.\tools\verify-toolchain.ps1
. .\tools\toolchain.ps1
$godotBin = Resolve-GodotBin
& $godotBin --headless --path (Get-Location).Path --quit-after 2
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

## Editor check

Open the repository in the pinned Godot .NET editor and run the project. Check
that the 1280 x 720 window shows a centered `Ancient History` title, an
`Offline` status, and build text, then stop it and confirm startup and shutdown
events appear once in the output.
