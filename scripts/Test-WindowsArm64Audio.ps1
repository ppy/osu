<#
.SYNOPSIS
Checks MP3 and Vorbis decoding with the published ARM64 BASS library.
.DESCRIPTION
Run in native ARM64 PowerShell 7 on Windows. Uses the no-sound device and existing
repository samples; it does not open an audio output device or start the game.
#>
[CmdletBinding()]
param(
    [string] $PublishDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/win-arm64')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT -or
    [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne 'Arm64') {
    throw 'Audio decoding checks require native ARM64 PowerShell 7 on Windows.'
}

if (-not ('OsuWindowsArm64.AudioSmokeTest' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OsuWindowsArm64
{
    public static class AudioSmokeTest
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool Init(int device, uint frequency, uint flags, IntPtr window, IntPtr clsid);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool Free();
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int Error();
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate uint Create(int memory, IntPtr data, ulong offset, ulong length, uint flags);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate uint GetData(uint stream, [Out] float[] buffer, uint length);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool FreeStream(uint stream);

        private static T Export<T>(IntPtr library, string name) where T : Delegate =>
            Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));

        public static void Run(string libraryPath, string[] samples)
        {
            IntPtr library = NativeLibrary.Load(libraryPath);
            try
            {
                var init = Export<Init>(library, "BASS_Init");
                var free = Export<Free>(library, "BASS_Free");
                var error = Export<Error>(library, "BASS_ErrorGetCode");
                var create = Export<Create>(library, "BASS_StreamCreateFile");
                var getData = Export<GetData>(library, "BASS_ChannelGetData");
                var freeStream = Export<FreeStream>(library, "BASS_StreamFree");
                if (!init(0, 44100, 0, IntPtr.Zero, IntPtr.Zero))
                    throw new InvalidOperationException("BASS_Init failed: " + error());
                try
                {
                    foreach (string sample in samples)
                    {
                        byte[] bytes = File.ReadAllBytes(sample);
                        GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                        uint stream = 0;
                        try
                        {
                            // Decode from bytes, without relying on the filename extension.
                            // BASS_STREAM_DECODE | BASS_STREAM_PRESCAN.
                            stream = create(1, pinned.AddrOfPinnedObject(), 0, (ulong)bytes.Length, 0x220000);
                            if (stream == 0)
                                throw new InvalidOperationException(sample + ": BASS stream creation failed: " + error());
                            var pcm = new float[65536];
                            uint count = getData(stream, pcm, 0x40000000 | (uint)(pcm.Length * sizeof(float)));
                            if (count == uint.MaxValue || count == 0 || count % sizeof(float) != 0)
                                throw new InvalidOperationException(sample + ": BASS decoding failed: " + error());
                            bool nonzero = false;
                            for (int i = 0; i < count / sizeof(float); i++)
                            {
                                if (float.IsNaN(pcm[i]) || float.IsInfinity(pcm[i]))
                                    throw new InvalidOperationException(sample + ": non-finite PCM");
                                nonzero |= pcm[i] != 0;
                            }
                            if (!nonzero)
                                throw new InvalidOperationException(sample + ": silent PCM");
                            Console.WriteLine("Decoded nonzero PCM: " + Path.GetFileName(sample));
                        }
                        finally
                        {
                            if (stream != 0) freeStream(stream);
                            pinned.Free();
                        }
                    }
                }
                finally { free(); }
            }
            finally { NativeLibrary.Free(library); }
        }
    }
}
'@
}

$sampleDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'osu.Game.Tests/Resources/Samples'
$samples = @('test-sample.mp3', 'test-sample.ogg') | ForEach-Object { Join-Path $sampleDirectory $_ }
$libraryPath = Join-Path (Resolve-Path -LiteralPath $PublishDirectory).Path 'bass.dll'
[OsuWindowsArm64.AudioSmokeTest]::Run($libraryPath, [string[]] $samples)
