# Windows ARM64

The desktop project can be built and published as a native ARM64 application for
Windows 11, including the Surface Pro X SQ1. The publish script produces a
self-contained portable build.

## Running a portable build

1. Install the [Microsoft Visual C++ ARM64 runtime](https://aka.ms/vc14/vc_redist.arm64.exe).
   BASS and the shader compilation library require it; a self-contained .NET
   publish does not include this runtime.
2. Copy the entire publish directory to a writable location on the device, or
   extract the complete ZIP archive.
3. Run `Start-osu-arm64.cmd`. Copying only `osu!.exe` is insufficient.

The launcher sets `OSU_EXTERNAL_UPDATE_PROVIDER` so that a locally published
ARM64 build is not replaced by an official Windows x64 update. To update the
build, publish again into a new directory. After changing an option that requires
a restart, such as the renderer, run the launcher again.

The Automatic renderer prefers Direct3D 11 on Windows, so an OpenGL compatibility
layer is not required. Performance and latency still need device-specific testing.

Debug builds use the separate `osu-development` data directory and development
server. Use Debug for development and testing. Release builds use the regular
game environment.

## Building from source

Install the .NET 10 SDK specified by `global.json`. Windows x64 supports
cross-publishing; on Windows ARM64, use the native ARM64 SDK. From the repository
root, run:

```powershell
./scripts/Publish-WindowsArm64.ps1
```

The default output is `artifacts/win-arm64`, including the .NET runtime, game and
native libraries. To publish a Release build into a separate directory:

```powershell
./scripts/Publish-WindowsArm64.ps1 -Configuration Release -OutputDirectory artifacts/win-arm64-release
```

Use `-DotNetPath` to select an SDK installed outside the default location. The
equivalent direct publish command is:

```powershell
dotnet publish osu.Desktop/osu.Desktop.csproj -c Debug -r win-arm64 --self-contained true -o artifacts/win-arm64
```

The direct `dotnet` command does not create the launcher. Before starting the
resulting application, set:

```powershell
$env:OSU_EXTERNAL_UPDATE_PROVIDER = 'local Windows ARM64 build'
```

## Platform support

- Supply the ARM64 `basswasapi.dll` missing from the pinned
  `ppy.osu.Framework.NativeLibs` package, retaining the default WASAPI backend.
- Override the package's ARM64 `bass.dll` with the upstream build that fixes Ogg
  Vorbis decoding on Windows ARM64. This also affects beatmap previews whose
  `.mp3` URLs return Vorbis audio. Both native libraries are temporary overrides
  until the framework native package includes them; see their
  [provenance and licence information](assets/native/win-arm64/README.md).
- Copy the overrides and licences only for Windows ARM64 executables, including
  desktop builds without an explicit RID when using an ARM64 Windows SDK.
  Explicit x64 and mobile builds do not use these overrides.
- Limit NVAPI loading to x86/x64 processes and use pointer-sized Windows keyboard
  hook handles and results.
- Validate the published PE architectures and required native libraries, and
  provide a Windows ARM64 CI publish and native library loading check.

The current NuGet dependencies already supply ARM64 Realm, SDL3, BASSmix, BASS FX,
FFmpeg, SQLite and Veldrid SPIRV libraries. StbiSharp has no Windows ARM64 native
library, but the framework falls back to ImageSharp for image decoding. Do not
copy x64 native DLLs into an ARM64 publish directory.

## Validation

Static architecture checks can run on either x64 or ARM64:

```powershell
./scripts/Test-WindowsArm64Publish.ps1 -PublishDirectory artifacts/win-arm64
```

In a native ARM64 PowerShell 7 process, also check whether the native libraries and
their system dependencies load:

```powershell
./scripts/Test-WindowsArm64Publish.ps1 -PublishDirectory artifacts/win-arm64 -LoadNativeLibraries
```

From a source checkout, also run the MP3 and Vorbis decoder regression check:

```powershell
./scripts/Test-WindowsArm64Audio.ps1 -PublishDirectory artifacts/win-arm64
```

This checks the existing repository samples using BASS's no-sound device and
requires native ARM64 PowerShell 7. CI runs it after the native library checks.

`-LoadNativeLibraries` requires a Windows ARM64 process and fails if run under an
unsupported architecture. The portable package includes the validation script;
from its extracted directory, run
`./Test-WindowsArm64Publish.ps1 -PublishDirectory . -LoadNativeLibraries` without
downloading the source. CI performs the same checks, but they do not replace
testing the game on a device.

Device testing on a Surface Pro X SQ1 running Windows 11 ARM64 confirmed native
startup and Direct3D 11 initialisation on the Adreno 680. The device owner
confirmed audible beatmap previews and playback of a previously silent beatmap
after replacing only `bass.dll` with the included upstream fix. Native decoder
checks also produced non-zero PCM for the tested Vorbis audio through file,
memory, callback and tempo/reverse paths. These results do not establish
compatibility with every audio format, output device or gameplay scenario.

Further device testing should cover first-run database creation, image and
beatmap import, speakers and headphones, WASAPI device switching, keyboard and
touch input, and video playback. If startup fails, retain the game logs and
Windows error message. For missing `VCRUNTIME140.dll` or `MSVCP140.dll`, check the
ARM64 Visual C++ runtime installation.

The official installer and update channels are maintained separately in
`ppy/osu-deploy`. A local publish does not provide an installer, signing or an
ARM64 automatic update service.
