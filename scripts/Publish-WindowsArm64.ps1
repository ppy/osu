<#
.SYNOPSIS
Publishes a self-contained, portable Windows ARM64 osu! build.
.DESCRIPTION
Run with the .NET SDK required by global.json. Cross-publishing from Windows x64
is supported. Debug is the default local build configuration; Release is also
supported. Existing output is not deleted. The complete output is checked for
missing dependencies and incompatible PE architectures before a launcher is made.
.PARAMETER Configuration
The build configuration (Debug by default).
.PARAMETER OutputDirectory
The publish directory. Defaults to artifacts/win-arm64 under the repository.
.PARAMETER DotNetPath
The dotnet command or absolute path to a dotnet executable.
.EXAMPLE
./scripts/Publish-WindowsArm64.ps1 -DotNetPath C:/dotnet/dotnet.exe
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/win-arm64'),

    [string] $DotNetPath = 'dotnet'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryDirectory = Split-Path $PSScriptRoot -Parent
$projectPath = Join-Path $repositoryDirectory 'osu.Desktop/osu.Desktop.csproj'
$publishDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$dotnetCommand = (Get-Command $DotNetPath -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source

# Run from the repository so global.json selects the intended SDK, including
# when this script is invoked from a different working directory.
Push-Location -LiteralPath $repositoryDirectory
try {
    & $dotnetCommand publish $projectPath --configuration $Configuration --runtime win-arm64 --self-contained true --output $publishDirectory -p:UseAppHost=true -p:PublishSingleFile=false -p:PublishTrimmed=false
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

& (Join-Path $PSScriptRoot 'Test-WindowsArm64Publish.ps1') -PublishDirectory $publishDirectory

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Test-WindowsArm64Publish.ps1') -Destination $publishDirectory -Force

foreach ($documentName in @('WINDOWS_ARM64.md', 'LICENCE')) {
    Copy-Item -LiteralPath (Join-Path $repositoryDirectory $documentName) -Destination (Join-Path $publishDirectory $documentName) -Force
}

# Keep the update-provider override local to this launcher and its child process.
# Disable delayed expansion because both the executable name and user paths can
# contain exclamation marks. %* preserves the caller's argument quoting.
$launcher = @'
@echo off
setlocal EnableExtensions DisableDelayedExpansion
set "OSU_EXTERNAL_UPDATE_PROVIDER=local Windows ARM64 build"
"%~dp0osu!.exe" %*
exit /b %errorlevel%
'@
[System.IO.File]::WriteAllText((Join-Path $publishDirectory 'Start-osu-arm64.cmd'), $launcher.Replace("`r`n", "`n").Replace("`n", "`r`n") + "`r`n", [System.Text.Encoding]::ASCII)

Write-Host "Windows ARM64 publish verified: $publishDirectory"
Write-Host 'On Windows 11 ARM64, run Start-osu-arm64.cmd to launch the local build.'
