Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectDir = (Resolve-Path -LiteralPath $PSScriptRoot).Path
. (Join-Path $projectDir 'tools/toolchain.ps1')

$godotBin = Resolve-GodotBin
$godotVersion = Get-GodotVersion -GodotBin $godotBin
Write-Host "Using Godot $godotVersion`: $godotBin"

& $godotBin '--editor' '--path' $projectDir @args
exit $LASTEXITCODE
