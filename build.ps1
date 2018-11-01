##########################################################################
# This is a customized Cake bootstrapper script for PowerShell.
##########################################################################

<#

.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script restores NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER ShowDescription
Shows description about tasks.
.PARAMETER DryRun
Performs a dry run.
.PARAMETER ScriptArgs
Remaining arguments are added here.

.LINK
https://cakebuild.net

#>

[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target,
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

Write-Host "Preparing to run build script..."

# Determine the script root for resolving other paths.
if(!$PSScriptRoot){
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

# Resolve the paths for resources used for debugging.
$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$CAKE_CSPROJ = Join-Path $TOOLS_DIR "cakebuild.csproj"

# Install the required tools locally.
Write-Host "Restoring cake tools..."
Invoke-Expression "dotnet restore `"$CAKE_CSPROJ`" --packages `"$TOOLS_DIR`"" | Out-Null

# Find the Cake executable
$CAKE_EXECUTABLE = (Get-ChildItem -Path ./tools/cake.coreclr/ -Filter Cake.dll -Recurse).FullName

# Build Cake arguments
$cakeArguments = @("$Script");
if ($Target) { $cakeArguments += "-target=$Target" }
if ($Configuration) { $cakeArguments += "-configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "-verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "-showdescription" }
if ($DryRun) { $cakeArguments += "-dryrun" }
if ($Experimental) { $cakeArguments += "-experimental" }
$cakeArguments += $ScriptArgs

# Start Cake
Write-Host "Running build script..."
Invoke-Expression "dotnet `"$CAKE_EXECUTABLE`" $cakeArguments"
exit $LASTEXITCODE
