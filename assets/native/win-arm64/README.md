# Windows ARM64 audio libraries

These unmodified upstream binaries fill two gaps in
`ppy.osu.Framework.NativeLibs` version `2025.806.0-nativelibs`:

- `basswasapi.dll` supplies the Windows ARM64 BASSWASAPI add-on. The package only
  includes x86 and x64 versions, while osu! enables this backend by default.
- `bass.dll` replaces the package's ARM64 BASS `2.4.17` with an upstream build
  that fixes Ogg Vorbis decoding on Windows ARM64. Beatmap previews can contain
  Vorbis audio even when their download URL ends in `.mp3`.

Remove these temporary overrides once the framework native package supplies
both the ARM64 BASSWASAPI library and a BASS version containing the Vorbis fix.

## BASS provenance

Retrieved from Un4seen Developments on 2026-09-17. The library author reproduced
the Windows ARM64 Vorbis failure and provided the updated binary in the
[upstream issue discussion](https://www.un4seen.com/forum/?topic=20998.0).

| File | Upstream archive | Archive member | SHA-256 of file |
| --- | --- | --- | --- |
| `bass.dll` | <https://www.un4seen.com/stuff/bass-arm64.zip> | `bass.dll` | `D1C009A03EDB076F448D9F0CC53DAE14133B279E38CC89C222942F7911B8E56A` |

Archive SHA-256:
`93DC0E5811EF5745CDA91A263E73EE5471A17B47F8D6EABA4212D0E278F95DDC`.

- PE machine: `0xAA64` (ARM64); size: 266,872 bytes.
- `BASS_GetVersion` reports `2.4.18.21`.
- On a Surface Pro X SQ1 running Windows 11 ARM64, the package's BASS `2.4.17`
  and the stable `2.4.18.3` rejected the tested Vorbis audio. This build decoded
  that audio through file, memory, callback and tempo/reverse paths with non-zero
  PCM output. The device owner also confirmed audible previews and playback of
  a previously silent beatmap after replacing only `bass.dll`.
- These are decoder and playback observations, not a claim that every diagnostic
  check passed: the diagnostic harness also reports an unsupported existing BASS
  FX custom tempo attribute (`0x10017`).

## BASSWASAPI and licence provenance

Retrieved from Un4seen Developments on 2026-09-16. Upstream download URLs are
mutable; verify the hashes before replacing any vendored file.

| File | Upstream archive | Archive member | SHA-256 of file |
| --- | --- | --- | --- |
| `basswasapi.dll` | <https://www.un4seen.com/files/bass24-arm64.zip> | `arm64/basswasapi.dll` | `8485FF195CDF682BA3EE7298EC4D758856459549EB5D2DA24830CEE7457AF3A1` |
| `basswasapi.txt` | <https://www.un4seen.com/files/basswasapi24.zip> | `basswasapi.txt` | `711E15FBB19A0032278D9FF8FC510AAA7C17761E7314EF7B742599AAC8E0F745` |
| `bass.txt` | <https://www.un4seen.com/files/bass24.zip> | `bass.txt` | `E2A6A97C41892F9DF78C367E7901CBE2262843E80EDD23882B721F50F7A9DE4B` |

Archive SHA-256 values:

- `bass24-arm64.zip`: `372282AF7D19F0D3F781177CE3BBDEC2FBD6876CC1B0B76D1CDDAA38D3DA4556`
- `basswasapi24.zip`: `74B754A925FEDED1DDD6B8B7B1A37BDC9B185E86ABEBDD46B1A7E0FF67815779`
- `bass24.zip`: `3A03EC9A33D0F4F9D167660DA51C8BB1432E8977496995455AB137277D69636E`

The ARM64 release is announced by the library author at
<https://www.un4seen.com/forum/?topic=20601>.

### BASSWASAPI compatibility

- PE machine: `0xAA64` (ARM64); file version: `2.4.4`; size: 39,472 bytes.
- Exported function names cover all 23 exports of the x64 BASSWASAPI `2.4.4`
  shipped in the pinned framework native package and all 23 P/Invoke entry
  points in `ppy.ManagedBass.Wasapi` version `2022.1216.0`.
- Upstream requires BASS `2.4.11` or later; the included BASS override meets this
  requirement.
- Imports include `bass.dll`, Windows system libraries, the Universal C Runtime
  and `VCRUNTIME140.dll`. The Microsoft Visual C++ runtime must be available for
  ARM64, as already required by the pinned ARM64 BASS and Veldrid SPIRV libraries.
  A successful cross-build does not establish audio-device compatibility;
  playback must also be tested on an ARM64 Windows device.

## Licence

These binaries are unmodified third-party components and are not covered by the
repository's MIT licence. The original upstream `basswasapi.txt` and `bass.txt`
are included unchanged. BASSWASAPI is free to use with BASS; the BASS licence and
usage terms still apply. Include both text files when distributing these binaries.
