Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:ToolchainRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$script:GodotVersionFile = Join-Path $script:ToolchainRoot 'tools/godot-version.txt'
$script:ExpectedDotnetVersion = '10.0.302'

function Get-ExpectedGodotVersion {
    if (-not (Test-Path -LiteralPath $script:GodotVersionFile -PathType Leaf)) {
        throw "Godot version file not found: $script:GodotVersionFile"
    }

    $version = (Get-Content -LiteralPath $script:GodotVersionFile -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "Godot version file is empty: $script:GodotVersionFile"
    }
    return $version
}

function Normalize-GodotVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RawVersion
    )

    $expected = Get-ExpectedGodotVersion
    $normalized = $RawVersion.Trim()
    $pattern = '^' + [regex]::Escape($expected) + '\.(official|custom_build)(?:\.[A-Za-z0-9]+)?$'

    $match = [regex]::Match($normalized, $pattern)
    if ($match.Success) {
        return "$expected.$($match.Groups[1].Value)"
    }
    throw "Unsupported Godot version: $RawVersion"
}

function Resolve-GodotBin {
    if (-not [string]::IsNullOrWhiteSpace($env:GODOT_BIN)) {
        $override = $env:GODOT_BIN
        if (Test-Path -LiteralPath $override -PathType Leaf) {
            return (Resolve-Path -LiteralPath $override).Path
        }

        $overrideCommand = Get-Command -Name $override -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $overrideCommand) {
            return $overrideCommand.Source
        }
        throw "GODOT_BIN does not point to an executable: $override"
    }

    foreach ($name in @('godot4-mono', 'godot-mono', 'godot4', 'godot', 'Godot.exe', 'godot.exe')) {
        $command = Get-Command -Name $name -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $command) {
            return $command.Source
        }
    }

    throw 'No Godot .NET editor found. Set GODOT_BIN or add one of the supported commands to PATH: godot4-mono, godot-mono, godot4, godot, Godot.exe, godot.exe'
}

function Get-GodotVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string] $GodotBin
    )

    $output = & $GodotBin '--version' 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        throw "Godot executable failed when queried with --version: $GodotBin"
    }
    return (Normalize-GodotVersion -RawVersion $output.Trim())
}

function Test-GodotDotnetEditor {
    param(
        [Parameter(Mandatory = $true)]
        [string] $GodotBin
    )

    $helpOutput = & $GodotBin '--help' 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        throw "Godot executable failed when queried with --help: $GodotBin"
    }
    if ($helpOutput -notmatch '(?m)(^|\s)--build-solutions(?:\s|=|$)') {
        throw "Godot executable does not expose the .NET editor option --build-solutions: $GodotBin"
    }
}

function Get-DotnetVersion {
    $dotnet = Get-Command -Name dotnet -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $dotnet) {
        throw 'dotnet was not found on PATH'
    }

    $output = & $dotnet.Source '--version' 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet --version failed'
    }
    $version = $output.Trim()
    if ($version -ne $script:ExpectedDotnetVersion) {
        throw "Unsupported .NET SDK version: $version (expected $script:ExpectedDotnetVersion)"
    }
    return $version
}

function Invoke-GodotHeadlessBuild {
    param(
        [Parameter(Mandatory = $true)]
        [string] $GodotBin,
        [Parameter(Mandatory = $true)]
        [string] $ProjectDir
    )

    Test-GodotDotnetEditor -GodotBin $GodotBin
    $output = & $GodotBin '--headless' '--editor' '--path' $ProjectDir '--build-solutions' '--quit' 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        throw "Godot .NET headless import/build check failed for $GodotBin`n$output"
    }
}
