Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $root 'tools/toolchain.ps1')

$failures = 0
function Pass([string] $Name) { Write-Output "PASS: $Name" }
function Fail([string] $Name) { Write-Output "FAIL: $Name"; $script:failures++ }

function Assert-Normalizes([string] $Input, [string] $Expected) {
    try {
        $actual = Normalize-GodotVersion -RawVersion $Input
        if ($actual -eq $Expected) { Pass "accepts $Input" } else { Fail "accepts $Input" }
    } catch { Fail "accepts $Input" }
}

function Assert-Rejects([string] $Input) {
    try {
        [void](Normalize-GodotVersion -RawVersion $Input)
        Fail "rejects $Input"
    } catch { Pass "rejects $Input" }
}

Assert-Normalizes '4.7.1.stable.mono.double.official' '4.7.1.stable.mono.double.official'
Assert-Normalizes '4.7.1.stable.mono.double.official.a13da4feb' '4.7.1.stable.mono.double.official'
Assert-Normalizes '4.7.1.stable.mono.double.custom_build' '4.7.1.stable.mono.double.custom_build'
Assert-Normalizes '4.7.1.stable.mono.double.custom_build.a13da4feb' '4.7.1.stable.mono.double.custom_build'
Assert-Rejects '4.7.1.stable'
Assert-Rejects '4.7.1.stable.official.a13da4feb'
Assert-Rejects '4.7.1.stable.mono.official.a13da4feb'
Assert-Rejects '4.7.1.stable.double.custom_build.a13da4feb'
Assert-Rejects '4.7.1.stable.mono.double.custom_build.'
Assert-Rejects '4.7.1.stable.mono.double.custom_build.ab-bad'
Assert-Rejects '4.7.1.stable.mono.double.custom_build.ab_bad'
Assert-Rejects '4.7.1.stable.mono.double.custom_build.ab.bad'
Assert-Rejects '4.7.1.stable.mono.double.custom_build.é'
Assert-Rejects '4.7.0.stable.mono.double.custom_build.a13da4feb'
Assert-Rejects '4.7.2.stable.mono.double.custom_build.a13da4feb'
Assert-Rejects '4.8.0.stable.mono.double.custom_build.a13da4feb'
Assert-Rejects '4.7.1.beta1.mono.double.custom_build.a13da4feb'

$missing = Join-Path ([IO.Path]::GetTempPath()) 'ancient-history-missing-godot.exe'
$oldOverride = $env:GODOT_BIN
try {
    $env:GODOT_BIN = $missing
    try { [void](Resolve-GodotBin); Fail 'missing GODOT_BIN fails deterministically' } catch { Pass 'missing GODOT_BIN fails deterministically' }
} finally {
    $env:GODOT_BIN = $oldOverride
}

$temp = Join-Path ([IO.Path]::GetTempPath()) ('ancient-history-toolchain-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $good = Join-Path $temp 'good-godot.cmd'
    $launchLog = Join-Path $temp 'launcher.log'
    Set-Content -LiteralPath $good -Encoding ASCII -Value @('@echo off', 'if "%~1"=="--version" (echo 4.7.1.stable.mono.double.custom_build.a13da4feb & exit /b 0)', 'if "%~1"=="--help" (echo --build-solutions & exit /b 0)', "echo %*>>`"$launchLog`"", 'if "%~1"=="--headless" exit /b 1', 'exit /b 0')
    $oldOverride = $env:GODOT_BIN
    try {
        $env:GODOT_BIN = $good
        $resolved = Resolve-GodotBin
        if ($resolved -eq (Resolve-Path -LiteralPath $good).Path) { Pass 'GODOT_BIN override is selected' } else { Fail 'GODOT_BIN override is selected' }
        if ((Get-GodotVersion -GodotBin $resolved) -eq '4.7.1.stable.mono.double.custom_build') { Pass 'PowerShell normalizes custom Godot output' } else { Fail 'PowerShell normalizes custom Godot output' }
        try { Invoke-GodotHeadlessBuild -GodotBin $resolved -ProjectDir $root; Fail 'unsupported Godot executable fails headless verification' } catch { Pass 'unsupported Godot executable fails headless verification' }
        & (Join-Path $root 'open_godot.ps1') '--test-argument' | Out-Null
        $launchOutput = Get-Content -LiteralPath $launchLog -Raw
        if ($launchOutput.Contains('--editor') -and $launchOutput.Contains('--test-argument')) { Pass 'PowerShell launcher opens the override executable' } else { Fail 'PowerShell launcher opens the override executable' }

        $ansiHelp = "$([char]27)[36m--build-solutions$([char]27)[0m  Build scripting solutions"
        try { Test-GodotBuildSolutionsHelp -HelpOutput $ansiHelp; Pass 'PowerShell accepts ANSI-colored --build-solutions help' } catch { Fail 'PowerShell accepts ANSI-colored --build-solutions help' }

        $standard = Join-Path $temp 'standard-godot.cmd'
        Set-Content -LiteralPath $standard -Encoding ASCII -Value @('@echo off', 'if "%~1"=="--version" (echo 4.7.1.stable.mono.double.custom_build.a13da4feb & exit /b 0)', 'if "%~1"=="--help" (echo standard editor help without .NET options & exit /b 0)', 'exit /b 0')
        try { Invoke-GodotHeadlessBuild -GodotBin $standard -ProjectDir $root; Fail 'standard non-.NET editor is rejected when it silently ignores --build-solutions' } catch { Pass 'standard non-.NET editor is rejected when it silently ignores --build-solutions' }
    } finally {
        $env:GODOT_BIN = $oldOverride
    }
} finally {
    Remove-Item -LiteralPath $temp -Recurse -Force
}

if ($failures -gt 0) { exit 1 }
