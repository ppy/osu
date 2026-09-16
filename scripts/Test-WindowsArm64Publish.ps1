<#
.SYNOPSIS
Checks a self-contained Windows ARM64 osu! publish without executing the game.
.DESCRIPTION
Checks the application, runtime, and required native dependencies, then reads PE
headers of every published DLL and EXE, including subdirectories. Native and
ReadyToRun images must target ARM64. Pure IL AnyCPU assemblies are allowed.
This check can run on Windows x64 or ARM64 without external tools.
.PARAMETER PublishDirectory
The publish directory. Defaults to artifacts/win-arm64 under the repository.
.PARAMETER LoadNativeLibraries
Also asks the Windows loader to load each required third-party native library,
resolving dependencies beside it. Requires a native Windows ARM64 PowerShell
process. This is a loader smoke test, not a graphics, audio, or gameplay test.
.EXAMPLE
./scripts/Test-WindowsArm64Publish.ps1 -PublishDirectory ./artifacts/win-arm64
.EXAMPLE
./scripts/Test-WindowsArm64Publish.ps1 -LoadNativeLibraries
#>
[CmdletBinding()]
param(
    [string] $PublishDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/win-arm64'),

    [switch] $LoadNativeLibraries
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-PeMetadata {
    param([Parameter(Mandatory = $true)][string] $Path)

    $stream = [System.IO.File]::OpenRead($Path)
    $reader = [System.IO.BinaryReader]::new($stream)
    try {
        function Read-PeUInt16([long] $Offset) {
            if ($Offset -lt 0 -or $Offset + 2 -gt $stream.Length) {
                throw "Truncated PE header: $Path"
            }
            $stream.Position = $Offset
            return $reader.ReadUInt16()
        }

        function Read-PeUInt32([long] $Offset) {
            if ($Offset -lt 0 -or $Offset + 4 -gt $stream.Length) {
                throw "Truncated PE header: $Path"
            }
            $stream.Position = $Offset
            return $reader.ReadUInt32()
        }

        if ((Read-PeUInt16 0) -ne 0x5A4D) {
            throw "Missing DOS/PE signature: $Path"
        }
        [long] $peOffset = Read-PeUInt32 0x3C
        if ((Read-PeUInt32 $peOffset) -ne 0x00004550) {
            throw "Invalid PE signature: $Path"
        }

        $machine = Read-PeUInt16 ($peOffset + 4)
        $sectionCount = Read-PeUInt16 ($peOffset + 6)
        $optionalHeaderSize = Read-PeUInt16 ($peOffset + 20)
        $optionalHeaderOffset = $peOffset + 24
        $magic = Read-PeUInt16 $optionalHeaderOffset
        switch ($magic) {
            0x10B { $directoryOffset = 96; $directoryCountOffset = 92 }
            0x20B { $directoryOffset = 112; $directoryCountOffset = 108 }
            default { throw "Unsupported PE optional header: $Path" }
        }
        if ($optionalHeaderSize -lt $directoryOffset -or $optionalHeaderOffset + $optionalHeaderSize -gt $stream.Length) {
            throw "Truncated PE optional header: $Path"
        }
        if ($machine -eq 0xAA64 -and $magic -ne 0x20B) {
            throw "ARM64 image is not PE32+: $Path"
        }

        [long] $clrRva = 0
        $directoryCount = Read-PeUInt32 ($optionalHeaderOffset + $directoryCountOffset)
        if ($directoryCount -gt 14) {
            if ($optionalHeaderSize -lt $directoryOffset + 15 * 8) {
                throw "Truncated CLR directory: $Path"
            }
            $clrRva = Read-PeUInt32 ($optionalHeaderOffset + $directoryOffset + 14 * 8)
            $clrSize = Read-PeUInt32 ($optionalHeaderOffset + $directoryOffset + 14 * 8 + 4)
            if (($clrRva -eq 0) -ne ($clrSize -eq 0)) {
                throw "Invalid CLR directory: $Path"
            }
        }

        [uint32] $clrFlags = 0
        $hasNativeManagedCode = $false
        if ($clrRva -ne 0) {
            if ($clrSize -lt 72) {
                throw "Truncated CLR header: $Path"
            }
            $clrFileOffset = $null
            for ($sectionIndex = 0; $sectionIndex -lt $sectionCount; $sectionIndex++) {
                $sectionOffset = $optionalHeaderOffset + $optionalHeaderSize + $sectionIndex * 40
                [long] $sectionRva = Read-PeUInt32 ($sectionOffset + 12)
                [long] $rawSize = Read-PeUInt32 ($sectionOffset + 16)
                [long] $rawOffset = Read-PeUInt32 ($sectionOffset + 20)
                [long] $relativeOffset = $clrRva - $sectionRva
                if ($relativeOffset -ge 0 -and $relativeOffset + 72 -le $rawSize) {
                    $clrFileOffset = $rawOffset + $relativeOffset
                    break
                }
            }
            if ($null -eq $clrFileOffset -or (Read-PeUInt32 $clrFileOffset) -lt 72) {
                throw "Cannot locate a valid CLR header: $Path"
            }
            $clrFlags = Read-PeUInt32 ($clrFileOffset + 16)
            $nativeHeaderRva = Read-PeUInt32 ($clrFileOffset + 64)
            $nativeHeaderSize = Read-PeUInt32 ($clrFileOffset + 68)
            $hasNativeManagedCode = $nativeHeaderRva -ne 0 -or $nativeHeaderSize -ne 0
        }

        # IMAGE_FILE_MACHINE_I386 with ILONLY and no 32-bit restriction is the
        # PE representation of pure IL AnyCPU. A ReadyToRun/native CLR header
        # makes the machine field significant even if ILONLY is also present.
        $isAnyCpu = $clrRva -ne 0 -and $machine -eq 0x014C -and ($clrFlags -band 1) -ne 0 -and ($clrFlags -band 0x20002) -eq 0 -and -not $hasNativeManagedCode
        return [PSCustomObject]@{
            Machine = $machine
            IsManaged = $clrRva -ne 0
            IsAnyCpu = $isAnyCpu
        }
    }
    finally {
        $reader.Dispose()
        $stream.Dispose()
    }
}

$publishRoot = (Resolve-Path -LiteralPath $PublishDirectory -ErrorAction Stop).ProviderPath
if (-not (Test-Path -LiteralPath $publishRoot -PathType Container)) {
    throw "Not a publish directory: $publishRoot"
}

$requiredNativeLibraries = @(
    'bass.dll', 'bassmix.dll', 'bass_fx.dll', 'basswasapi.dll',
    'realm-wrappers.dll', 'SDL3.dll', 'libveldrid-spirv.dll', 'e_sqlite3.dll',
    'avutil-56.dll', 'avcodec-58.dll', 'avformat-58.dll', 'swscale-5.dll'
)
$requiredNativeFiles = @('osu!.exe', 'coreclr.dll', 'hostfxr.dll', 'hostpolicy.dll') + $requiredNativeLibraries
$requiredFiles = @('osu!.dll', 'osu!.deps.json', 'osu!.runtimeconfig.json', 'System.Private.CoreLib.dll') + $requiredNativeFiles
$failures = [System.Collections.Generic.List[string]]::new()
foreach ($requiredFile in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishRoot $requiredFile) -PathType Leaf)) {
        $failures.Add("Missing required file: $requiredFile")
    }
}

$binaryCount = 0
$nativeCount = 0
foreach ($binary in (Get-ChildItem -LiteralPath $publishRoot -Recurse -File | Where-Object { $_.Extension -in '.dll', '.exe' })) {
    try {
        $metadata = Read-PeMetadata -Path $binary.FullName
        $binaryCount++
        if (-not $metadata.IsManaged) {
            $nativeCount++
        }
        if ($metadata.Machine -ne 0xAA64 -and -not $metadata.IsAnyCpu) {
            $failures.Add(('Incompatible PE machine 0x{0:X4}: {1}' -f $metadata.Machine, $binary.FullName))
        }
        if ($binary.DirectoryName.TrimEnd([char[]] '\/') -eq $publishRoot.TrimEnd([char[]] '\/') -and $binary.Name -in $requiredNativeFiles) {
            if ($metadata.Machine -ne 0xAA64 -or $metadata.IsManaged) {
                $failures.Add("Expected a native ARM64 image: $($binary.Name)")
            }
        }
    }
    catch {
        $failures.Add($_.Exception.Message)
    }
}

if ($failures.Count -gt 0) {
    throw ("Windows ARM64 publish validation failed:`n - " + ($failures -join "`n - "))
}

if ($LoadNativeLibraries) {
    if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) {
        throw '-LoadNativeLibraries requires Windows and a native ARM64 PowerShell process.'
    }
    if (-not ('OsuWindowsArm64.NativeLoader' -as [type])) {
        Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
namespace OsuWindowsArm64
{
    public static class NativeLoader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process2(IntPtr process, out ushort processMachine, out ushort nativeMachine);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string fileName, IntPtr file, uint flags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr module);
    }
}
'@
    }
    [ushort] $processMachine = 0
    [ushort] $nativeMachine = 0
    if (-not [OsuWindowsArm64.NativeLoader]::IsWow64Process2([OsuWindowsArm64.NativeLoader]::GetCurrentProcess(), [ref] $processMachine, [ref] $nativeMachine)) {
        throw [System.ComponentModel.Win32Exception]::new([System.Runtime.InteropServices.Marshal]::GetLastWin32Error())
    }
    if ($nativeMachine -ne 0xAA64 -or ($processMachine -ne 0 -and $processMachine -ne 0xAA64)) {
        throw '-LoadNativeLibraries requires a native Windows ARM64 PowerShell process; an x64 or x86 process cannot load ARM64 DLLs.'
    }

    $libraryHandles = [System.Collections.Generic.List[IntPtr]]::new()
    try {
        foreach ($libraryName in $requiredNativeLibraries) {
            $libraryPath = Join-Path $publishRoot $libraryName
            # LOAD_WITH_ALTERED_SEARCH_PATH searches the loaded DLL's directory
            # for imports such as BASS and the other FFmpeg libraries.
            $libraryHandle = [OsuWindowsArm64.NativeLoader]::LoadLibraryEx($libraryPath, [IntPtr]::Zero, 0x00000008)
            if ($libraryHandle -eq [IntPtr]::Zero) {
                $loaderError = [System.Runtime.InteropServices.Marshal]::GetLastWin32Error()
                $loaderMessage = [System.ComponentModel.Win32Exception]::new($loaderError).Message
                $failures.Add("Cannot load $libraryName (Windows error $loaderError): $loaderMessage")
            }
            else {
                $libraryHandles.Add($libraryHandle)
            }
        }
    }
    finally {
        for ($handleIndex = $libraryHandles.Count - 1; $handleIndex -ge 0; $handleIndex--) {
            if (-not [OsuWindowsArm64.NativeLoader]::FreeLibrary($libraryHandles[$handleIndex])) {
                $failures.Add("FreeLibrary failed with Windows error $([System.Runtime.InteropServices.Marshal]::GetLastWin32Error()).")
            }
        }
    }
    if ($failures.Count -gt 0) {
        throw ("Windows ARM64 native loading failed:`n - " + ($failures -join "`n - "))
    }
    Write-Host "Loaded all $($requiredNativeLibraries.Count) required third-party native libraries successfully."
}

Write-Host "Validated $binaryCount PE files ($nativeCount native) for Windows ARM64 in $publishRoot"
