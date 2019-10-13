[CmdletBinding()]
Param(
    [string]$Target,
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [Parameter(Position = 0, Mandatory = $false, ValueFromRemainingArguments = $true)]
    [string[]]$ScriptArgs
)

# Build Cake arguments
$cakeArguments = "";
if ($Target) { $cakeArguments += "-target=$Target" }
if ($Configuration) { $cakeArguments += "-configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "-verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "-showdescription" }
if ($DryRun) { $cakeArguments += "-dryrun" }
if ($Experimental) { $cakeArguments += "-experimental" }
$cakeArguments += $ScriptArgs

dotnet tool install Cake.Tool --global --version 0.35.0
dotnet cake ./build/build.cake --bootstrap
dotnet cake ./build/build.cake $cakeArguments
exit $LASTEXITCODE