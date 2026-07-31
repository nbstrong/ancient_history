Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectDir = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
. (Join-Path $PSScriptRoot 'toolchain.ps1')

try {
    $dotnetVersion = Get-DotnetVersion
    $godotBin = Resolve-GodotBin
    $godotVersion = Get-GodotVersion -GodotBin $godotBin
    Invoke-GodotHeadlessBuild -GodotBin $godotBin -ProjectDir $projectDir

    Write-Output "dotnet: $dotnetVersion"
    Write-Output "godot: $godotVersion"
    Write-Output "godot executable: $godotBin"
    Write-Output 'headless import/build: passed'
}
catch {
    Write-Error "Toolchain verification failed: $($_.Exception.Message)"
    exit 1
}
