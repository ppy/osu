BASS 2.4
Copyright (c) 1999-2025 Un4seen Developments Ltd. All rights reserved.


BASS is an audio library for use in software on several platforms. Its purpose
is to provide developers with powerful and efficient sample, stream, MOD music,
and recording functions. All in a compact DLL that won't bloat your software.

Main Features
-------------
* Samples
  supports WAV/AIFF/MP3/MP2/MP1/OGG and custom generated samples

* Sample streams
  stream any sample data in 8/16/32 bit, with both "push" and "pull" systems

* File streams
  MP3/MP2/MP1/OGG/WAV/AIFF file streaming

* Internet file streaming
  stream files from the internet, including Shout/Icecast

* User file streaming
  stream files from anywhere using any delivery method

* OS codecs
  ACM, Media Foundation and CoreAudio codec support for additional audio formats

* Multi-channel streaming
  support for more than plain stereo, including multi-channel OGG/WAV/AIFF files

* MOD music
  uses the same engine as XMPlay = best accuracy, speed, and quality

* MO3 music
  MP3/OGG compressed MOD music

* Add-on system
  support for more formats is available via add-ons (aka plugins)

* Multiple outputs
  simultaneously use multiple soundcards, and move channels between them

* Recording
  flexible recording system, with support for multiple devices

* Decode without playback
  streams and MOD musics can be outputted in any way you want

* Speaker assignment
  assign streams and MOD musics to specific speakers

* High precision synchronization
  synchronize events in your software to the streams and MOD musics

* Effects
  chorus/compressor/distortion/echo/flanger/gargle/parameq/reverb

* User-defined DSP functions
  custom effects may be applied to individual streams or the final output mix

* 32 bit floating-point decoding and processing
  floating-point stream/music decoding, DSP/FX, and recording

* 3D sound
  play samples/streams/musics in any 3D position


Requirements
============
Win32 version
-------------
Windows 98 or newer is required with an SSE-supporting CPU. BASS can use WASAPI
or DirectSound for playback and recording. WASAPI is the default on Windows
Vista and newer, while DirectSound is on older Windows versions. 32-bit and
64-bit Windows are supported. ARM64 support is available separately at the BASS
website.

macOS version
-------------
macOS 10.6 or above is required. The arm64 / x86_64 / i386 architectures are
supported in a single universal library.

Linux version
-------------
BASS requires ALSA for playback and recording, but decoding and silent playback
on the "No sound" device is still possible without it. The x86 / x86_64 /
armhf / aarch64 architectures are supported. The x86 build requires SSE, while
the armhf build requires ARMv6+VFP2 or above.

Android version
---------------
Android 2.3 or above is required. BASS can use AAudio, OpenSL ES, or AudioTrack
for playback, and AAudio or OpenSL ES for recording. AAudio is the default on
Android 8.1 and above, while OpenSL ES is on older Android versions. The
armeabi-v7a / arm64-v8a / x86 / x86_64 architectures are supported.

iOS version
-----------
iOS 8 or above is required. armv7 / armv7s / arm64 "device" builds and
i386 / x86_64 / arm64 "simulator" builds are supported in a single universal
framework.


Using BASS
==========
See the documentation for descriptions of all the BASS functions. Also check
the included example source-codes for some examples of how to use BASS in your
own programs.

There is no guarantee that all future BASS versions will be compatible with all
previous versions, so your program should use BASS_GetVersion to check the
version that is loaded. This also means that you should put the BASS module in
the same directory as your executable (not just somewhere in the path), to
avoid the possibility of a wrong version being loaded.

If you are updating your software from a previous BASS version, then you should
check the "History" section (below), to see if any of the functions that you
are using have been affected by a change. Also see the "Upgrading" section of
the documentation.

NOTE: To view the documentation outside of Windows, you will need a CHM file
      viewer. The documentation can also be viewed online at the BASS website.

Win32 version
-------------
C/C++, Delphi, and Visual Basic APIs and example projects are provided. To
use BASS with another language you will first have to convert the header
file. Or, as a last resort, you could use LoadLibrary and GetProcAddress.

The downside is that you have to manually import each function that you
use, with the GetProcAddress function. But it has been made a lot simpler
to import BASS this way by the use of the BASSDEF #define. Here's a small
example:

#define BASSDEF(f) (WINAPI *f) // define the functions as pointers
#include "bass.h"
...
HINSTANCE bass=LoadLibrary("BASS.DLL"); // load BASS
BASS_Init=GetProcAddress(bass,"BASS_Init"); // get BASS_Init
BASS_Init(-1,44100,0,hWnd,NULL); // call BASS_Init

To use BASS with Borland C++ Builder, you will first have to create a Borland
C++ Builder import library for it. This is done by using the IMPLIB tool that
comes with Borland C++ Builder. Simply execute this:

	IMPLIB BASSBCB.LIB BASS.DLL

... and then use BASSBCB.LIB in your projects to import BASS.

To use BASS with LCC-Win32, you will first have to create a compatible import
library for it. This is done by using the PEDUMP and BUILDLIB tools that come
with LCC-Win32. Run these 2 commands:

	PEDUMP /EXP BASS.LIB > BASSLCC.EXP
	BUILDLIB BASSLCC.EXP BASSLCC.LIB

... and then use BASSLCC.LIB in your projects to import BASS.

For the BASS functions that return strings (char*), VB users should use the
VBStrFromAnsiPtr function to convert the returned pointer into a VB string.

macOS version
-------------
LIBBASS.DYLIB is a universal binary with support for the x86_64 / arm64 / i386
architectures. If not all of those architectures are needed, the included
makefile can be used to extract the wanted one(s). Type "make x86_64" or
"make arm64" or "make i386" to extract a particular architecture. "make 64bit"
can be used to extract the x86_64 and arm64 architectures (exclude i386), and
"make intel" to extract the x86_64 and i386 architectures (exclude arm64). Any
BASS add-ons placed in the same directory will also be processed.

NOTE: You will need to exclude the i386 architecture (run "make 64bit") for
      submitting an app to Apple's App Store.

Since BASS 2.4.17, the BASS/add-on libraries use the "@rpath" install path,
while "@loader_path was used by older versions. If upgrading from an old
version, you may need to change your copy/embed destination to "Frameworks"
rather than "Executables", unless your "Runtime Search Paths" setting includes
"@executable_path" or "@loader_path". The included makefile ("make rpath") can
be used to update older BASS/add-on versions to use "@rpath".

See the example Xcode project.

Linux version
-------------
The LIBBASS.SO file needs to be in the OS's library search path for it to
be found; simply placing it in the same directory as the executable won't
necessarily work as it does on other platforms. The BASS libraries should
also not be placed in the standard library directories to avoid version
conflicts. To meet those competing requirements, a wrapper script could be
used to set the "LD_LIBRARY_PATH" environment variable before loading the
executable. Another option is to set the "rpath" in the executable to tell
it where to look for libraries, which is what the example makefiles do.

When building for multiple architectures, it is possible to simply try linking
with all LIBBASS.SO versions, and the linker will use the appropriate one and
ignore the others.

Android version
---------------
The BASS library includes support for use in Java and native code. The
BASS.JAVA file mirrors BASS.H as much as possible, but there are some
unavoidable differences, mainly in parameter/return types.

When using ProGuard to obfuscate your Java code, the BASS classes should be
left alone (see PROGUARD-BASS.PRO).

See the example Android Studio project.

iOS version
-----------
The BASS XCFramework should be added to XCode's "Frameworks, Libraries, and
Embedded Content" section. If you're using an old Xcode version without
XCFramework support then you can instead directly use the "device" or
"simulator" framework from within the XCFramework. BASS does not include
bitcode, so the "Enable Bitcode" option should be set to "No".

Swift
-----
To use BASS with Swift, you will need a bridging header referenced in your
project's "Objective-C Bridging Header" setting, which includes "#import"
lines for BASS.H and any add-on headers. See Apple's documentation for more
information on bridging headers.


Included files
==============
Win32 version
-------------
BASS.TXT        This file
BASS.DLL        32-bit BASS module
BASS.CHM        BASS documentation
X64\
  BASS.DLL        64-bit BASS module
C\
  BASS.H          BASS C/C++ header file
  BASS.LIB        32-bit BASS import library
  BASS.SLN        Visual Studio solution for examples
  MAKEFILE        Makefile for examples
  MAKEFILE.IN     Makefile helper macros
  X64\
    BASS.LIB        64-bit BASS import library
  3DTEST\         3D example
    ...
  BASSTEST\       Simple playback example
    ...
  CONTEST\        Console example
    ...
  CUSTLOOP\       Custom looping example
    ...
  DEVLIST\        Device list example
    ...
  DSPTEST\        DSP example
    ...
  FXTEST\         Effects example
    ...
  LIVEFX\         Full-duplex example
    ...
  LIVESPEC\       Live version of spectrum analyser example
    ...
  MODTEST\        MOD music example
    ...
  MULTI\          Multiple device example
    ...
  NETRADIO\       Internet streaming example
    ...
  PLUGINS\        Plugins example
    ...
  RECTEST\        Recording example
    ...
  SPEAKERS\       Multi-speaker example
    ...
  SPECTRUM\       Spectrum analyser example
    ...
  SYNTH\          Low latency custom streaming example
    ...
  WRITEWAV\       WAVE writer example
    ...
  BIN\            Precompiled examples
    3DTEST.EXE
    BASSTEST.EXE
    CONTEST.EXE
    CUSTLOOP.EXE
    DEVLIST.EXE
    DSPTEST.EXE
    FXTEST.EXE
    LIVEFX.EXE
    LIVESPEC.EXE
    MODTEST.EXE
    MULTI.EXE
    NETRADIO.EXE
    RECTEST.EXE
    SPEAKERS.EXE
    SPECTRUM.EXE
    SYNTH.EXE
    WRITEWAV.EXE
DELPHI\
  BASS.PAS        BASS Delphi/Pascal unit
  3DTEST\         3D example
    ...
  BASSTEST\       Simple playback example
    ...
  CONTEST\        Console example
    ...
  CUSTLOOP\       Custom looping example
    ...
  DSPTEST\        DSP example
    ...
  FXTEST\         Effects example
    ...
  LIVEFX\         Full-duplex example
    ...
  LIVESPEC\       Live version of spectrum analyser example
    ...
  MULTI\          Multiple device example
    ...
  NETRADIO\       Internet streaming example
    ...
  PLUGINS\        Plugins example
    ...
  RECTEST\        Recording example
    ...
  SAMPLEVIS\      Visualisation example
    ...
  SPEAKERS\       Multi-speaker example
    ...
  SPECTRUM\       Spectrum analyser example
    ...
  STREAMTEST\     User stream example
    ...
  WRITEWAV\       WAVE writer example
    ...
VB\
  BASS.BAS        BASS Visual Basic module
  3DTEST\         3D example
    ...
  BASSTEST\       Simple playback example
    ...
  CUSTLOOP\       Custom looping example
    ...
  DEVLIST\        Device list example
    ...
  DSPTEST\        DSP example
    ...
  FXTEST\         Effects example
    ...
  LIVEFX\         Full-duplex example
    ...
  LIVESPEC\       Live version of spectrum analyser example
    ...
  MODTEST\        MOD music example
    ...
  MULTI\          Multiple device example
    ...
  NETRADIO\       Internet streaming example
    ...
  PLUGINS\        Plugins example
    ...
  RECTEST\        Recording example
    ...
  SPEAKERS\       Multi-speaker example
    ...
  SPECTRUM\       Spectrum analyser example
    ...
  SYNTH\          Low latency custom streaming example
    ...
  WRITEWAV\       WAVE writer example
    ...
  MEMORY\         Playing from memory example by Jobnik
    ...

NOTE: You may need to "Unblock" the BASS.CHM file in its "Properties" to view
      it. The documentation can also be viewed online at the BASS website.

NOTE: To run the example EXEs, first you will need to copy BASS.DLL into the
      same directory as them.

NOTE: The Delphi and VB examples may not always be up to date with the C
      examples, so the latter should be preferred as references if possible.

macOS version
-------------
BASS.TXT        This file
LIBBASS.DYLIB   BASS module
BASS.CHM        BASS documentation
MAKEFILE        Makefile for specific architectures
C\
  BASS.H          BASS C/C++/Objective-C header file
  BASS.XCODEPROJ  Xcode project for examples
  CONTEST\        Console example
    ...
  DEVLIST\        Device list example
    ...
  DSPTEST\        DSP example
    ...
  FXTEST\         Effects example
    ...
  LIVESPEC\       Live version of spectrum analyser example
    ...
  MULTI\          Multiple device example
    ...
  NETRADIO\       Internet streaming example
    ...
  PLUGINS\        Plugins example
    ...
  RECTEST\        Recording example
    ...
  SPEAKERS\       Multi-speaker example
    ...
  SPECTRUM\       Spectrum analyser example
    ...
  SYNTH\          Low latency custom streaming example
    ...
  WRITEWAV\       WAVE writer example
    ...
DELPHI\
  BASS.PAS        BASS Delphi/Pascal unit
  DEMO\
    ...

Linux version
-------------
BASS.TXT        This file
BASS.CHM        BASS documentation
LIBS\           BASS modules
  ...
C\
  BASS.H          BASS C/C++ header file
  MAKEFILE        Makefile for examples
  MAKEFILE.IN     Makefile helper macros
  3DTEST\         3D example
    ...
  BASSTEST\       Simple playback example
    ...
  CONTEST\        Console example
    ...
  CUSTLOOP\       Custom looping example
    ...
  DEVLIST\        Device list example
    ...
  DSPTEST\        DSP example
    ...
  FXTEST\         Effects example
    ...
  LIVESPEC\       Live version of spectrum analyser example
    ...
  MODTEST\        MOD music example
    ...
  MULTI\          Multiple device example
    ...
  NETRADIO\       Internet streaming example
    ...
  PLUGINS\        Plugins example
    ...
  RECTEST\        Recording example
    ...
  SPEAKERS\       Multi-speaker example
    ...
  SPECTRUM\       Spectrum analyser example
    ...
  SYNTH\          Low latency custom streaming example
    ...
  WRITEWAV\       WAVE writer example
    ...
DELPHI\
  BASS.PAS        BASS Delphi/Pascal unit

Android version
---------------
BASS.TXT        This file
BASS.CHM        BASS documentation
LIBS\           BASS modules
  ...
C\
  BASS.H          BASS C/C++ header file
JAVA\COM\UN4SEEN\BASS\
  BASS.JAVA       BASS Java class
EXAMPLES\       Java examples
  BUILD.GRADLE    Android Studio build script for examples
  SETTINGS.GRADLE
  PROGUARD-BASS.PRO
  DSPTEST\        DSP example
    ...
  FXTEST\         Effects example
    ...
  LIVESPEC\       Live version of spectrum analyser example
    ...
  NETRADIO\       Internet streaming example
    ...
  PLUGINS\        Plugins example
    ...
  RECTEST\        Recording example
    ...
  SPECTRUM\       Spectrum analyser example
    ...
  BASSLIB\        BASS library wrapper shared by examples
    ...

iOS version
-----------
BASS.TXT        This file
BASS.XCFRAMEWORK  BASS framework
BASS.CHM        BASS documentation
BASS.H          BASS C/C++/Objective-C header file


Latest Version
==============
The latest version of BASS can always be found at the BASS website:

	www.un4seen.com


Licence
=======
BASS is free for non-commercial use. If you are a non-commercial entity
(eg. an individual) and you are not making any money from your product
(through sales/advertising/etc), then you can use BASS in it for free.
If you wish to use BASS in commercial products, then please also see the
next section.

TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, BASS IS PROVIDED
"AS IS", WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY
AND/OR FITNESS FOR A PARTICULAR PURPOSE. THE AUTHORS SHALL NOT BE HELD
LIABLE FOR ANY DAMAGE THAT MAY RESULT FROM THE USE OF BASS. YOU USE
BASS ENTIRELY AT YOUR OWN RISK.

Usage of BASS indicates that you agree to the above conditions.

All trademarks and other registered names contained in the BASS
package are the property of their respective owners.

Commercial licensing
--------------------
BASS is available for use in your commercial products. The licence types
available are as follows:

SHAREWARE: Allows the usage of BASS in an unlimited number of your shareware
("try before you buy") products, which must sell for no more than 40 EUR each.
Non-shareware products are also permitted, but the price limit is 10 EUR in
that case. With subscription models, the limit applies to the annual
subscription fee. The limit can be raised by purchasing duplicate licences (for
the same platform), eg. 2 licences doubles it. If you are an individual (not a
corporation) making and selling your own software, this is the licence for you.

SINGLE COMMERCIAL: Allows the usage of BASS in one commercial product.

UNLIMITED COMMERCIAL: Allows the usage of BASS in an unlimited number of your
commercial products. This licence is on a per-site basis, eg. if you are
creating products with BASS at 2 sites/locations, then 2 licences are required.

Please note the products must be end-user products, eg. not components used by
other products. 

These licences only cover your own software, not the publishing of other's
software. If you publish other's software, its developers (or the software
itself) will need to be licensed to use BASS.

These licences are on a per-platform basis, with reductions available when
licensing for multiple platforms. In all cases there are no royalties to pay,
and you can use future BASS updates without further cost.

These licences do not allow reselling/sublicensing of BASS. For example, if a
product is a development system, the users of said product are not licensed to
use BASS in their productions; they will need their own licences.

If the standard licences do not meet your requirements, or if you have any
questions, please get in touch (email: bass@un4seen.com).

Visit the BASS website for the latest pricing:

	www.un4seen.com


Credits
=======
MP3 decoding is based on minimp3, https://github.com/lieff/minimp3

Ogg Vorbis decoding is based on libogg/vorbis,
Copyright (c) 2002-2020 Xiph.org Foundation

API/Sample contributors
-----------------------
Delphi: Titus Miloi, Rogier Timmermans, Alessandro Cappellozza,
        Jesse Naranjo, Chris Troesken, Evgeny Melnikov
Visual Basic: Adam Hoult, Hendrik Knaepen, Arthur Aminov,
              Peter Hebels, Juan Pablo Franchi


History
=======
These are the major (and not so major) changes in each release, along with the
functions/options that are introduced/affected by them. There are other little
tweaks and fixes made along the way too.

2.4.18.3 - 15/12/2025
---------------------
* Support for MP3/MP2/MP1 data in a single track MP4 container
	BASS_StreamCreateFile/User/URL
* Specific error code for seeking failures that leave a stream unable to continue
	BASS_ERROR_ENDED (BASS_ChannelSetPosition error code)
* Tweaks for internet/buffered streaming with Media Foundation codecs
	BASS_StreamCreateURL
	BASS_StreamCreateFileUser
	BASS_ChannelSetPosition
* Fix for missing iTunes MP4 tags bug in 2.4.18
	BASS_TAG_MP4 (BASS_ChannelGetTags type)
* Fix for burst of old sound when restarting a stopped device on Windows
	BASS_Start

2.4.18 - 7/10/2025
------------------
* Downmixed multi-channel playback
	BASS_CONFIG_DOWNMIX (BASS_SetConfig option)
	BASS_ATTRIB_DOWNMIX (BASS_ChannelSetAttribute option)
* Multi-channel DX8 effect support
	BASS_ChannelSetFX
* DSP/FX on individual mono/stereo channels within multi-channel streams
	BASS_ChannelSetDSPEx
	BASS_DSP_MONO_N/BASS_DSP_STEREO_N (BASS_ChannelSetDSPEx/FX flags)
* DSP/FX bypassing
	BASS_DSP_BYPASS (BASS_ChannelSetDSPEx/FX flag)
	BASS_FXSetBypass
* Floating-point DSP functions
	BASS_DSP_FLOAT (BASS_ChannelSetDSPEx flag)
* Read-only DSP functions
	BASS_DSP_READONLY (BASS_ChannelSetDSPEx flag)
* Freed/removed DSP function notifications
	BASS_DSP_FREECALL (BASS_ChannelSetDSPEx flag)
* DSP/FX freeing without needing a channel handle
	BASS_FXFree
	FXTEST example updated
* DSP/FX processing granularity support on decoding channels
	BASS_ATTRIB_GRANULE (BASS_ChannelSetAttribute option)
* DSP callback position retrieval
	BASS_POS_DSP (BASS_ChannelGetPosition mode)
* Channels can be assigned to unavailable speakers
	BASS_SPEAKER_xxx (BASS_MusicLoad/BASS_StreamCreate/File/URL flags)
* Speaker flags can be added/removed after channel creation and set to unavailable ones
	BASS_ChannelFlags
* Multi-speaker output enabling on Android
	BASS_DEVICE_SPEAKERS (BASS_Init flag)
* Multi-speaker output on "No Sound" device
	BASS_DEVICE_SPEAKERS (BASS_Init flag)
* Configurable "No sound" device update delay limit
	BASS_CONFIG_NOSOUND_MAXDELAY (BASS_SetConfig option)
* Device list notifications on Windows/macOS
	BASS_CONFIG_DEV_NOTIFY (BASS_SetConfigPtr option)
	DEVICENOTIFYPROC
* User handling of all local file reading
	BASS_CONFIG_FILEOPENPROCS (BASS_SetConfigPtr option)
	FILEOPENPROC
	BASS_FILEOPENPROCS structure
* Monotonic byte position (unaffected by seeking) retrieval and syncing
	BASS_POS_RAW (BASS_ChannelGetPosition mode)
	BASS_SYNC_POS_RAW (BASS_ChannelSetSync type)
* Custom end position is rounded-up to nearest sample
	BASS_POS_END (BASS_ChannelGetPosition mode)
* Retrieval of custom end and loop position settings
	BASS_POS_END (BASS_ChannelGetPosition mode)
	BASS_POS_LOOP (BASS_ChannelGetPosition mode)
* Integer attribute values
	BASS_ChannelSetAttributeEx
	BASS_ChannelGetAttributeEx
	BASS_ATTRIBTYPE_FLOAT
	BASS_ATTRIBTYPE_INT
* Unlimited user-defined attribute size
	BASS_ATTRIB_USER (BASS_ChannelSetAttributeEx option)
* DSP volume priority attribute retrieval
	BASS_ATTRIB_VOLDSP_PRIORITY (BASS_ChannelGetAttribute/Ex option)
* Setting an attribute cancels sliding of it
	BASS_ChannelSetAttribute/Ex
	BASS_ChannelSlideAttribute
* RECORDPROC option for doing nothing with the data
	RECORDPROC_TRUE (BASS_RecordStart option)
	LIVESPEC example updated
* Constant added for recording without a RECORDPROC
	RECORDPROC_NONE (BASS_RecordStart option)
* Minimum recording period reduced to 1ms
	BASS_RecordStart
* A recording channel is no longer auto-freed when it fails
	BASS_RecordStart
	BASS_SYNC_DEV_FAIL (BASS_ChannelSetSync type)
* ID3v2 tag block retrieval with its length
	BASS_TAG_ID3V2_BINARY (BASS_ChannelGetTags type)
	BASS_TAG_ID3V2_2_BINARY (BASS_ChannelGetTags type)
	TAG_BINARY structure
	BASS_TAG_ID3V2 deprecated
	BASS_TAG_ID3V2_2 deprecated
* MP4 cover art retrieval
	BASS_TAG_MP4_COVERART (BASS_ChannelGetTags type)
* MOD music filename retrieval
	BASS_CHANNELINFO filename member
* Internet stream creation cancelling
	BASS_StreamCancel
	NETRADIO example updated
* File reading "mem" option changed to a multi-option "filetype"
	BASS_FILE_NAME
	BASS_FILE_MEM
	BASS_StreamCreateFile
	BASS_SampleLoad
	BASS_MusicLoad
* Option to use a copy when streaming a file from memory
	BASS_FILE_MEMCOPY (BASS_StreamCreateFile option)
* Streaming from file handles/descriptors
	BASS_FILE_HANDLE
	BASS_StreamCreateFile
	BASS_SampleLoad
	BASS_MusicLoad
* ParcelFileDescriptors are duplicated so they can be reused on Android
	BASS_StreamCreateFile
	BASS_SampleLoad
	BASS_MusicLoad
* Thread-specific playback buffer option
	BASS_CONFIG_THREAD (BASS_SetConfig flag)
	BASS_CONFIG_BUFFER (BASS_SetConfig option)
* Thread-specific asynchronous file reading buffer option
	BASS_CONFIG_THREAD (BASS_SetConfig flag)
	BASS_CONFIG_ASYNCFILE_BUFFER (BASS_SetConfig option)
* Retrieval of asynchronous file reading buffer size
	BASS_FILEPOS_ASYNCSIZE (BASS_StreamGetFilePosition mode)
* Channel reference counting for delayed/asynchronous freeing
	BASS_ChannelRef
	BASS_ChannelFree
	BASS_StreamFree
	BASS_MusicFree
	BASS_TAG_INCREF (BASS_ChannelGetTags flag)
* Channels can be freed (asynchronously) within their callback functions
	BASS_ChannelFree
	BASS_StreamFree
	BASS_MusicFree
* Freeing a locked channel disables other calls on it while waiting for unlock
	BASS_ChannelLock
	BASS_ChannelFree
	BASS_StreamFree
	BASS_MusicFree
* Improved MP3 seeking accuracy without prescanning
	BASS_ChannelSetPosition
* Improved performance of playback/recording data retrieval
	BASS_ChannelGetData
	BASS_ChannelGetLevel/Ex
* 16 point SRC is used by default when NEON is available
	BASS_CONFIG_SRC (BASS_SetConfig option)
* Option to play audio from videos now applies to all formats (may require add-on updates)
	BASS_CONFIG_MF_VIDEO renamed to BASS_CONFIG_VIDEO (BASS_SetConfig option)
* Option to disable all Android Media codecs except AAC/ADTS
	BASS_CONFIG_AM_DISABLE (BASS_SetConfig option)
* Apple voice processing for echo cancellation on macOS/iOS
	BASS_DEVICE_APPLEVOICE (BASS_Init flag)
* Stereo output limiting on macOS
	BASS_DEVICE_STEREO (BASS_Init flag)
* Initialization flag for OpenSL ES output on Android
	BASS_DEVICE_OPENSLES (BASS_Init flag)
* AAudio recording on Android (OpenSL ES is fallback/optional)
	BASS_RECORD_OPENSLES (BASS_RecordStart flag)
* Native recording format retrieval on Android (8.1 and above)
	BASS_RecordGetInfo
	RECTEST example updated
* Supported recording formats info removed on old Windows (as on other platforms)
	BASS_RECORDINFO formats member
* Unused structure members removed (changed to "reserved")
	BASS_INFO structure
	BASS_SAMPLE structure
* buffer parameter must be 0 when using BASS_DATA_AVAILABLE flag
	BASS_ChannelGetData
* Support for custom "Host" request header on Linux/Android/UWP (already on other platforms)
	BASS_StreamCreateURL
* libssl.so.3 added to OpenSSL library search list on Linux
	BASS_StreamCreateURL
	BASS_CONFIG_LIBSSL (BASS_SetConfigPtr option)
* 16KB page support on Android
* Fix for bug in Doppler effect from listener velocity
	BASS_Set3DPosition
* Channel order fix for 7/8 channel OGG files
	BASS_StreamCreateFile/User/URL
* Fix for Media Foundation FLAC decoder seeking freeze
	BASS_ChannelSetPosition

2.4.17 - 12/10/2022
-------------------
* "Default" recording device on Windows/macOS
	BASS_RecordInit
	BASS_RecordGetDeviceInfo
	BASS_CONFIG_REC_DEFAULT (BASS_SetConfig option)
* Recording continues after device format changes on Windows/macOS
	BASS_RecordStart
	BASS_SYNC_DEV_FORMAT (BASS_ChannelSetSync type)
* DSP volume control
	BASS_ATTRIB_VOLDSP (BASS_ChannelSetAttribute option)
	BASS_ATTRIB_VOLDSP_PRIORITY (BASS_ChannelSetAttribute option)
	RECTEST example updated
* Auto-reconnecting when a download stops before the end
	BASS_StreamCreateURL
* Internet streams use HTTP/1.1 instead of HTTP/1.0
	BASS_StreamCreateURL
* Download callback changing after stream creation
	BASS_ATTRIB_DOWNLOADPROC (BASS_ChannelSetAttributeEx option)
	NETRADIO example updated
* Thread-specific internet/buffered stream download buffer option
	BASS_CONFIG_THREAD (BASS_SetConfig flag)
	BASS_CONFIG_NET_BUFFER (BASS_SetConfig option)
	BASS_StreamCreateURL/FileUser
* Internet stream creation never waits for pre-buffering
	BASS_CONFIG_NET_PREBUF_WAIT disabled
	BASS_StreamCreateURL/FileUser
* Stalled stream resumption buffer level defaults to prebuffer level
	BASS_ATTRIB_NET_RESUME (BASS_ChannelSetAttribute option)
* New simplified channel starting/playing function
	BASS_ChannelStart
* Channel remapping fix for various multi-channel source/output combinations
	BASS_ChannelStart/Play
* Support for using common add-on name instead of platform-specific filename in plugin loading
	BASS_PluginLoad
	NETRADIO example updated
* Library filename/path retrieval
	BASS_CONFIG_FILENAME (BASS_GetConfigPtr option)
	PLUGINS and WRITEWAV examples updated
* Device info is still available if reinitialization fails
	BASS_GetInfo
	BASS_DEVICE_REINIT (BASS_Init flag)
* Playback ramping-in/out can be disabled separately
	BASS_ATTRIB_NORAMP (BASS_ChannelSetAttribute option)
* Default playback ramping setting
	BASS_CONFIG_NORAMP (BASS_SetConfig option)
* Error code for when access is denied
	BASS_ERROR_DENIED
	BASS_StreamCreateURL
	BASS_RecordStart
* REAR2 speaker flags renamed/duplicated to SIDE
	BASS_SPEAKER_SIDE/SIDELEFT/SIDERIGHT (BASS_MusicLoad/BASS_StreamCreate/File/URL flags)
* Switch to dynamic library/framework (rather than static) on iOS
	BASS_PluginLoad
* Recording routing audio session options on iOS
	BASS_IOS_SESSION_AIRPLAY/BTHFP/BTA2DP (BASS_CONFIG_IOS_SESSION flags)
* Audio session deactivation when nothing is playing/recording on iOS
	BASS_IOS_SESSION_DEACTIVATE (BASS_CONFIG_IOS_SESSION flag)
* Interrupted status retrieval on iOS
	BASS_IsStarted
* Floating-point recording support below Android 5 (already supported above)
	BASS_RecordStart
* Internet stream support for Linux systems without /etc/services file
	BASS_StreamCreateURL
* Fix for IPv6 addresses in URLs on Windows/Linux/Android
	BASS_StreamCreateURL
* Fix for getting volume changes made externally on Linux
	BASS_GetVolume
	BASS_RecordGetInput
* Media Foundation Matroska support disabled due to a leak in it (use BASSWEBM instead)
* macOS version now uses "@rpath" instead of "@loader_path" install path
* ARM builds are now included (instead of separate) on Linux
* Non-FPU ARM architecture support dropped (Windows CE and "softfp" Linux)
* SSE is required on x86 platforms (Windows 95 dropped)

2.4.16 - 24/6/2021
------------------
* DirectSound output mix is now generated by BASS (as with WASAPI output and
  on other platforms), losing support for DirectSound-specific features but
  gaining support for all other BASS features
	BASS_DEVICE_DSOUND (BASS_Init flag)
	BASS_SAMPLE_FX/SOFTWARE/VAM *ignored*
* Length trimming
	BASS_POS_END (BASS_ChannelSetPosition mode)
	BASS_SYNC_END (BASS_ChannelSetSync type)
	CUSTLOOP and WRITEWAV examples updated
* Custom loop start positions
	BASS_POS_LOOP (BASS_ChannelSetPosition mode)
	CUSTLOOP example updated
* Length extending
	BASS_ATTRIB_TAIL (BASS_ChannelSetAttribute option)
	BASS_SYNC_END (BASS_ChannelSetSync type)
* Device reinitialization while retaining BASS channels and 3D settings
	BASS_DEVICE_REINIT (BASS_Init flag)
* Retrieval of a device's activity status
	BASS_IsStarted
* BASS_DEVICE_3D flag is no longer needed to enable 3D support
	BASS_Init
* The "Default" device is now enabled by default on Windows/macOS
	BASS_CONFIG_DEV_DEFAULT (BASS_SetConfig option)
* Default communications device indicator on Windows
	BASS_DEVICE_DEFAULTCOM (BASS_DEVICEINFO flag)
* Sample channel "newonly" option changed to a flag
	BASS_SAMCHAN_NEW (BASS_SampleGetChannel flag)
* Sample playback streams
	BASS_SAMCHAN_STREAM (BASS_SampleGetChannel flag)
	BASS_CTYPE_STREAM_SAMPLE
* Speaker assignment support for samples
	BASS_SampleGetChannel
	BASS_ChannelFlags
* Overriding of default flags during sample channel creation
	BASS_SampleGetChannel
* Same sample channel handle when max=1 is now optional
	BASS_CONFIG_SAMPLE_ONEHANDLE (BASS_SetConfig option)
* 3D max distance muting can now be toggled
	BASS_SAMPLE_MUTEMAX (BASS_ChannelFlags flag)
* Plugin disabling without affecting existing streams
	BASS_PluginEnable
* WASAPI recording on Windows (DirectSound is fallback/optional)
	BASS_CONFIG_REC_WASAPI (BASS_SetConfig option)
	BASS_RecordInit
* Minimum recording buffer reduced to 10ms
	BASS_CONFIG_REC_BUFFER (BASS_SetConfig option)
* Recording data/level retrieval without removing from buffer when not using a RECORDPROC
	BASS_DATA_NOREMOVE (BASS_ChannelGetData flag)
	BASS_LEVEL_NOREMOVE (BASS_ChannelGetLevelEx flag)
* Support for level measurements longer than 1s from decoding channels
	BASS_ChannelGetLevelEx
* Configurable restricted download rate
	BASS_CONFIG_NET_RESTRATE (BASS_SetConfig option)
* Thread-specific internet stream timeout options
	BASS_CONFIG_THREAD (BASS_SetConfig flag)
	BASS_CONFIG_NET_TIMEOUT (BASS_SetConfig option)
	BASS_CONFIG_NET_READTIMEOUT (BASS_SetConfig option)
	BASS_StreamCreateURL
* Configured "User-Agent" header can be overridden in custom request headers
	BASS_StreamCreateURL
* Multiple IPs will be tried when available on Linux/Android (already on other platforms)
	BASS_StreamCreateURL
* BASS_STREAM_BLOCK applied automatically on files larger than 2GB
	BASS_StreamCreateURL/FileUser
* Redirected URL retrieval
	BASS_TAG_LOCATION (BASS_ChannelGetTags type)
* Shoutcast metadata is now optional
	BASS_CONFIG_NET_META (BASS_SetConfig option)
	BASS_TAG_META (BASS_ChannelGetTags type)
* Support for a 2nd ID3v2 tag block
	BASS_TAG_ID3V2_2 (BASS_ChannelGetTags type)
* OpenMPT channel name retrieval
	BASS_TAG_MUSIC_CHAN (BASS_ChannelGetTags type)
* Built-in support for MP4 tags on all platforms
	BASS_TAG_MP4 (BASS_ChannelGetTags type)
* Delayed OGG file scanning when prescanning is disabled/impossible
	BASS_ChannelGetLength
	BASS_ChannelSetPosition
	BASS_CONFIG_OGG_PRESCAN (BASS_SetConfig option)
* Faster seeking in large OGG files
	BASS_ChannelSetPosition
* Unified freeing function for all channel types
	BASS_ChannelFree
* User-defined attribute
	BASS_ATTRIB_USER (BASS_ChannelSetAttributeEx option)
* Push stream buffer size limiting
	BASS_ATTRIB_PUSH_LIMIT (BASS_ChannelSetAttribute option)
* Retrieval of a stream's available file data amount
	BASS_FILEPOS_AVAILABLE (BASS_StreamGetFilePosition mode)
* Error code for unsupported URL protocols
	BASS_ERROR_PROTOCOL (BASS_StreamCreateURL error code)
* BASS_ERROR_ALREADY error changed to success in some functions
	BASS_ChannelPause
	BASS_ChannelSetDevice
	BASS_ChannelSetLink
* Support for custom file streaming with Android Media codecs
	BASS_StreamCreateFileUser
* Switch to using native APIs for Android Media codecs (Android 5 or above required now)
	BASS_StreamCreateFile/User/URL
* BASS_TAG_AM_MIME value changed to fix conflict on Android
	BASS_TAG_AM_MIME (BASS_ChannelGetTags type)
* Improved support for single active input recording devices on Linux
	BASS_RecordGetInputName
	BASS_RecordGetInput
	BASS_RecordSetInput
* Automatic resumption of recordings after an interruption on iOS
* Fix for Media Foundation AAC streaming with Windows 10 20H1
	BASS_StreamCreateURL
	BASS_StreamCreateFileUser
* Fix for freeze when freeing a stream after BASS_ChannelSetLink failed with BASS_ERROR_ALREADY on it
	BASS_ChannelSetLink
* macOS examples rewritten in Objective-C/Cocoa
* "MP3-FREE" version removed (MP3 patents expired)
* armv5 (armeabi) architecture support dropped on Android

2.4.15 - 17/12/2019
-------------------
* Channel processing granularity option
	BASS_ATTRIB_GRANULE (BASS_ChannelSetAttribute option)
* Asynchronous mixtime sync callbacks
	BASS_SYNC_THREAD (BASS_ChannelSetSync flag)
* Output device update period can be set in samples
	BASS_CONFIG_DEV_PERIOD (BASS_SetConfig option)
* Continuous output when nothing is playing is now optional on macOS/iOS too
	BASS_CONFIG_DEV_NONSTOP (BASS_SetConfig option)
* AAudio output support on Android
	BASS_Init
	BASS_CONFIG_ANDROID_AAUDIO (BASS_SetConfig option)
* Support for audio session IDs on Android
	BASS_CONFIG_ANDROID_SESSIONID (BASS_SetConfig option)
* ParcelFileDescriptor support on Android
	BASS_StreamCreateFile
	BASS_SampleLoad
	BASS_MusicLoad
* FloatValue Java class added to receive floating-point values on Android 10
	BASS_Get3DFactors
	BASS_RecordGetInput
	BASS_ChannelGetAttribute
	BASS_ChannelGet3DAttributes
* Audio session configuration option on iOS
	BASS_CONFIG_IOS_SESSION (BASS_SetConfig option)
* OGG channel type constant renamed/duplicated to VORBIS
	BASS_CTYPE_STREAM_VORBIS
* Error code for recognised file formats without audio
	BASS_ERROR_NOTAUDIO (BASS_StreamCreateFile/User/URL error code)
* Error code for unstreamable MP4 files due to atom order
	BASS_ERROR_UNSTREAMABLE (BASS_StreamCreateFileUser/URL error code)
* Fix for AAC streaming with BASS_STREAM_BLOCK on macOS 10.15 and iOS 13
	BASS_StreamCreateURL/FileUser
* Fix for playback rate changing bug when using DirectSound
	BASS_ATTRIB_FREQ (BASS_ChannelSetAttribute option)
* armv6 architecture support dropped on iOS

2.4.14 - 16/1/2019
------------------
* Support for playback on the "No sound" device
	BASS_Init
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad/Create
	BASS_ChannelPlay
* Improved sample rate conversion quality, including 64 point option
	BASS_ATTRIB_SRC (BASS_ChannelSetAttribute option)
	BASS_CONFIG_SRC/_SAMPLE (BASS_SetConfig options)
* Loopback (output device) recording on Windows Vista and newer
	BASS_RecordInit
	BASS_RecordGetDeviceInfo
	BASS_DEVICE_LOOPBACK (BASS_DEVICEINFO flag)
	DEVLIST and RECTEST examples updated
* Native format recording option
	BASS_RecordStart
* Access to the 3D output mix for applying DSP/FX
	STREAMPROC_DEVICE_3D (BASS_StreamCreate option)
* Retrieval of a device's started/stopped status
	BASS_IsStarted
* Sync for when a device stops unexpectedly
	BASS_SYNC_DEV_FAIL (BASS_ChannelSetSync type)
* New channel playback state for a paused device
	BASS_ACTIVE_PAUSED_DEVICE (BASS_ChannelIsActive)
* Sync for when a device's sample format changes
	BASS_SYNC_DEV_FORMAT (BASS_ChannelSetSync type)
* Adjustable output device update period
	BASS_CONFIG_DEV_PERIOD (BASS_SetConfig option)
* Nyquist FFT option
	BASS_DATA_FFT_NYQUIST (BASS_ChannelGetData flag)
* Plugin-specific flags are ignored in standard stream creation functions
	BASS_StreamCreateFile/User/URL
* Pre-buffering during stream creation is now disabled by default
	BASS_CONFIG_NET_PREBUF_WAIT (BASS_SetConfig option)
	BASS_StreamCreateURL/FileUser
* Existing internet streams are unaffected by read timeout config changes
	BASS_CONFIG_NET_READTIMEOUT (BASS_SetConfig option)
* Retrieval of the number of existing handles
	BASS_CONFIG_HANDLES (BASS_GetConfig option)
* Windows mixer settings are retained by default and optionally not
	BASS_CONFIG_WASAPI_PERSIST (BASS_SetConfig option)
* Configurable OpenSSL library filename on Linux/Android
	BASS_CONFIG_LIBSSL (BASS_SetConfigPtr option)
* Files can be deleted/renamed while streaming on Windows (already on other platforms)
	BASS_StreamCreateFile
* Fix for bug in playback buffering disabling option
	BASS_ATTRIB_NOBUFFER (BASS_ChannelSetAttribute option)
* Fix to support long logarithmic curves in volume effects
	BASS_FX_VOLUME (BASS_ChannelSetFX type)

2.4.13.8 - 6/2/2018
-------------------
* Indication of a file's original sample format being floating-point
	BASS_ORIGRES_FLOAT (BASS_CHANNELINFO/SAMPLE flag)
* Some extra sample data is retained to account for device latency in data retrieval
	BASS_ChannelGetData
* FFT window is shortened to match amount of available data when less than FFT size
	BASS_ChannelGetData
* FFT level adjusted to be approx the same regardless of amount of available data
	BASS_ChannelGetData
* Fix for crash when decoding channels are dissociated from a device
	BASS_NODEVICE (BASS_ChannelSetDevice option)

2.4.13 - 19/12/2017
-------------------
* WASAPI output on Windows (DirectSound is fallback/optional)
	BASS_DEVICE_DSOUND (BASS_Init flag)
	BASS_CONFIG_DEV_BUFFER/NONSTOP (BASS_SetConfig options)
	BASS_CONFIG_SRC/_SAMPLE (BASS_SetConfig options)
	BASS_ATTRIB_SRC/NORAMP (BASS_ChannelSetAttribute options)
* Default output device following support extended to Windows Vista and macOS
	BASS_CONFIG_DEV_DEFAULT (BASS_SetConfig option)
* Seamless playback buffer length changing
	BASS_ATTRIB_BUFFER (BASS_ChannelSetAttribute option)
	BASS_ATTRIB_NOBUFFER deprecated
	SYNTH example updated
* Access to the final output mix for applying DSP/FX
	STREAMPROC_DEVICE (BASS_StreamCreate option)
	BASS_CTYPE_STREAM_DEVICE
	FXTEST example updated
* Dissociating of decoding channels from a device
	BASS_NODEVICE (BASS_ChannelSetDevice option)
* Volume attribute levels above 1.0 for amplification
	BASS_ATTRIB_VOL (BASS_ChannelSetAttribute option)
* Logarithmic attribute slides
	BASS_SLIDE_LOG (BASS_ChannelSlideAttribute flag)
* Volume & panning attribute affected level retrieval
	BASS_LEVEL_VOLPAN (BASS_ChannelGetLevelEx flag)
* Volume level adjustment effect
	BASS_FX_VOLUME (BASS_ChannelSetFX type)
	BASS_FX_VOLUME_PARAM structure
* Asynchronous pre-buffering
	BASS_CONFIG_NET_PREBUF_WAIT (BASS_SetConfig option)
	BASS_StreamCreateURL/FileUser
	CONTEST and NETRADIO examples updated
* Decoding channels can be dissociated from a device
	BASS_ChannelSetDevice
* Resetting of user file streams/decoders
	BASS_POS_RESET (BASS_ChannelSetPosition flag)
* Relative seeking
	BASS_POS_RELATIVE (BASS_ChannelSetPosition flag)
* RIFF cue points chunk retrieval
	BASS_TAG_RIFF_CUE (BASS_ChannelGetTags type)
	TAG_CUE structure
* RIFF sample chunk retrieval
	BASS_TAG_RIFF_SMPL (BASS_ChannelGetTags type)
	TAG_SMPL structure
* Support for Category/Description/Grouping/Keywords/Lyrics MP4 tags
	BASS_TAG_MP4 (BASS_ChannelGetTags type)
* Separate channel type for dummy streams
	BASS_CTYPE_STREAM_DUMMY
* Stalled internet stream buffering progress retrieval
	BASS_FILEPOS_BUFFERING (BASS_StreamGetFilePosition mode)
	CONTEST and NETRADIO examples updated
* Playlist recursion depth config option
	BASS_CONFIG_NET_PLAYLIST_DEPTH (BASS_SetConfig options)
* Push file system read functions can now return 0 without ending the file
	STREAMFILE_BUFFERPUSH (BASS_StreamCreateFileUser system)
	FILEREADPROC
* 16 point SRC is used by default when SSE is available
	BASS_CONFIG_SRC (BASS_SetConfig option)
* Mono downmixing for all file formats in sample loading
	BASS_SAMPLE_MONO (BASS_SampleLoad flag)
* Mono OGG stream downmixing
	BASS_SAMPLE_MONO (BASS_StreamCreateFile/User/URL flag)
* Media Foundation availability detection
	BASS_CONFIG_MF_DISABLE (BASS_GetConfig option)
	PLUGINS example updated
* AudioTrack output on Android
	BASS_DEVICE_AUDIOTRACK (BASS_Init flag)
* Support for Android Media codecs
	BASS_StreamCreateFile/User/URL
	BASS_TAG_AM_MIME/NAME (BASS_ChannelGetTags type)
	BASS_CONFIG_AM_DISABLE (BASS_SetConfig option)
* Panning fix for MOD files created with ScreamTracker 3
	BASS_MusicLoad
* Fix for compressor and distotion DX8 effects with floating-point data on Windows
	BASS_FX_DX8_COMPRESSOR/DISTORTION (BASS_ChannelSetFX types)
* Fix for plugin freeing issue on 64-bit platforms (HPLUGIN is not library address any more)
	BASS_PluginLoad
	BASS_PluginFree

2.4.12 - 10/3/2016
------------------
* DSP/FX chain order modification
	BASS_FXSetPriority
* Average bitrate retrieval
	BASS_ATTRIB_BITRATE (BASS_ChannelGetAttribute option)
* Ramping-out when stopping/pausing playback
	BASS_ChannelStop
	BASS_ChannelPause
* Disabling of playback ramping
	BASS_ATTRIB_NORAMP (BASS_ChannelSetAttribute option)
* Pre-buffering when playback buffering is disabled
	BASS_ChannelUpdate
	BASS_ATTRIB_NOBUFFER (BASS_ChannelSetAttribute option)
* Net agent and proxy config options now make a copy of the provided string
	BASS_CONFIG_NET_AGENT/PROXY (BASS_SetConfigPtr options)
	NETRADIO example updated
* Unicode net agent and proxy config options on Windows
	BASS_CONFIG_NET_AGENT/PROXY (BASS_SetConfigPtr options)
* 32768 sample FFT
	BASS_DATA_FFT32768 (BASS_ChannelGetData flag)
* Support for iTunes gapless info in MP3 files
	BASS_StreamCreateFile/User
* 529 sample delay removed from start of MP3 files without delay info
	BASS_StreamCreateFile/User/URL
* Support for numerical MP4 genre (gnre) tags
	BASS_TAG_MP4 (BASS_ChannelGetTags type)
* Support for BASSWMA's tag option when using Media Foundation WMA codec
	BASS_TAG_WMA (BASS_ChannelGetTags type)
* OpenMPT author tag retrieval
	BASS_TAG_MUSIC_AUTH (BASS_ChannelGetTags type)
* FastTracker2 panning for XM files
	BASS_MUSIC_FT2PAN (BASS_MusicLoad flag)
* Support for longer device names on Windows
	BASS_DEVICEINFO name member
* Checking for floating-point support on platforms that might not have it
	BASS_CONFIG_FLOAT (BASS_GetConfig option)
* Attribute size retrieval fix
	BASS_ChannelGetAttributeEx
* Fix for missing LC_VERSION_MIN issue with Xcode 7 on OSX, ppc architecture (PowerPC) support dropped
* LIST example renamed to DEVLIST

2.4.11 - 22/12/2014
-------------------
* Support for custom HTTP request headers
	BASS_StreamCreateURL
* HTTPS support on Linux/Android
	BASS_StreamCreateURL
* Redirection HTTP headers provided during connection
	BASS_STREAM_STATUS (BASS_StreamCreateURL flag)
	DOWNLOADPROC
* Adjustable stalled internet stream resumption buffer level
	BASS_ATTRIB_NET_RESUME (BASS_ChannelSetAttribute option)
* Extended level retrieval inc. multi-channel and RMS options
	BASS_ChannelGetLevelEx
* Inexact/faster seeking
	BASS_POS_INEXACT (BASS_ChannelSetPosition flag)
* Post-stream creation scanning (seek table building)
	BASS_POS_SCAN (BASS_ChannelSetPosition flag)
* Extended attribute support
	BASS_ChannelSetAttributeEx
	BASS_ChannelGetAttributeEx
* Scanned info retrieval and setting
	BASS_ATTRIB_SCANINFO (BASS_ChannelGet/SetAttributeEx option)
* Built-in ADPCM WAVE file support on Linux/Android/WinCE
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad
* Active MOD channel count retrieval
	BASS_ATTRIB_MUSIC_ACTIVE (BASS_ChannelGetAttribute option)
* Fractional sample rates
	BASS_ATTRIB_FREQ (BASS_ChannelSetAttribute option)
* Device type information
	BASS_DEVICE_TYPE_xxx (BASS_DEVICEINFO flags)
* Unicode support for recording input names
	BASS_CONFIG_UNICODE (BASS_SetConfig option)
	BASS_RecordGetInputName
* File size retrieval
	BASS_FILEPOS_SIZE (BASS_StreamGetFilePosition mode)
* Asynchronous file reading buffer level retrieval
	BASS_FILEPOS_ASYNCBUF (BASS_StreamGetFilePosition mode)
* MPEG data search limit raised to 1MB
	BASS_CONFIG_VERIFY (BASS_SetConfig option)
* Separate MPEG data search limit for internet/buffered streams
	BASS_CONFIG_VERIFY_NET (BASS_SetConfig option)
* Support for iTunes gapless info with Media Foundation and CoreAudio codecs
	BASS_StreamCreateFile/User
* Airplay receiver enumeration and selection on OSX
	BASS_DEVICES_AIRPLAY (BASS_GetDeviceInfo flag)
	BASS_CONFIG_AIRPLAY (BASS_SetConfig option)
* Media Foundation playback of audio from videos is now optional
	BASS_CONFIG_MF_VIDEO (BASS_SetConfig option)
* Optional floating-point or fixed-point data retrieval on Android/WinCE
	BASS_DATA_FLOAT/FIXED (BASS_ChannelGetData flags)
* Continuous output when nothing is playing on Linux/Android/WinCE
	BASS_CONFIG_DEV_NONSTOP (BASS_SetConfig option)
* Fix for decoding channels being ended when stopping the output device
	BASS_Stop
* Fix for Media Foundation internet/buffered streaming on Windows 8
	BASS_StreamCreateURL
	BASS_StreamCreateFileUser
* Improved compatibility with recent PulseAudio versions on Linux
* LIST and MODTEST examples added
* MASM API and examples removed (available as separate download)

2.4.10 - 16/2/2013
------------------
* Asynchronous file reading/buffering
	BASS_ASYNCFILE (BASS_StreamCreateFile/User flag)
	BASS_CONFIG_ASYNCFILE_BUFFER (BASS_SetConfig option)
* Support for chained OGG files (multiple logical bitstreams)
	BASS_StreamCreateFile/User
	BASS_CONFIG_OGG_PRESCAN (BASS_SetConfig option)
	BASS_POS_OGG (BASS_ChannelSetPosition/ChannelGetLength mode)
* OGG logical bitstream syncs are no longer automatically "mixtime"
	BASS_SYNC_OGG_CHANGE (BASS_ChannelSetSync type)
* Support for Soundtracker MOD files
	BASS_MusicLoad
* Complex FFT data
	BASS_DATA_FFT_COMPLEX (BASS_ChannelGetData flag)
* Volume levels above 1.0 for decoding channels
	BASS_ATTRIB_VOL (BASS_ChannelSetAttribute option)
* Recording device channel count retrieval on Windows/OSX/iOS
	BASS_RECORDINFO formats member
* Minimum recording period reduced to 5ms
	BASS_RecordStart
* Recording device period adjustment on OSX
	BASS_RecordStart
* Device UID retrieval on OSX
	BASS_DEVICEINFO driver member
* The desktop window handle is used when none is provided
	BASS_Init

2.4.9 - 3/5/2012
----------------
* Improved and adjustable sample rate conversion quality
	BASS_ATTRIB_SRC (BASS_ChannelSetAttribute option)
	BASS_CONFIG_SRC/_SAMPLE (BASS_SetConfig options)
* Support for buffered file & internet streams with CoreAudio codecs on OSX/iOS
	BASS_StreamCreateURL
	BASS_StreamCreateUser
* Data and level retrieval from sample channels
	BASS_ChannelGetData
	BASS_ChannelGetLevel
* Oldest sample channel will be overridden when there are multiple candidates
	BASS_SampleGetChannel
* Use of the device's output rate with MODs
	BASS_MusicLoad
* Optional device output rate setting on OSX/iOS
	BASS_DEVICE_FREQ (BASS_Init flag)
* UTF-8 device information on Windows
	BASS_CONFIG_UNICODE (BASS_SetConfig option)
* Speaker assignment with panning/balance control on Windows Vista/7
	BASS_CONFIG_VISTA_SPEAKERS (BASS_SetConfig option)
	BASS_ATTRIB_PAN (BASS_ChannelGet/Set/SlideAttribute option)
* Push stream queue level can be checked after using BASS_STREAMPROC_END
	BASS_StreamPutData
* Support for ALSA subdevices on Linux
	BASS_Init
	BASS_GetDeviceInfo
	BASS_RecordInit
	BASS_RecordGetDeviceInfo
* Device list fix on 64-bit OSX
	BASS_GetDeviceInfo
	BASS_RecordGetDeviceInfo
* BASS_UNICODE support fix on Linux
	BASS_PluginLoad
* Internet streaming tweaks/fixes on Windows
* SYNTH example updated to use device output rate
* RECTEST example updated (C version) for better Windows Vista/7 support
* Documentation updated with platform-specific info for Android

2.4.8 - 29/8/2011
-----------------
* Support for Media Foundation codecs on Windows 7 and updated Vista
	BASS_StreamCreateFile/User/URL
	BASS_TAG_MF (BASS_ChannelGetTags type)
	PLUGINS example updated
* Built-in support for MP4 tags on Win32/OSX/iOS (for Media Foundation & CoreAudio AAC/ALAC codecs)
	BASS_TAG_MP4 (BASS_ChannelGetTags type)
* WAVE format chunk retrieval
	BASS_TAG_WAVEFORMAT (BASS_ChannelGetTags type)
* Adjustable sample channel limit
	BASS_SampleSetInfo
* Per-channel CPU usage monitoring
	BASS_ATTRIB_CPU (BASS_ChannelGetAttribute option)
* Pre-allocation of push stream queue space
	BASS_StreamPutData
* Support for AIFF files with the data chunk before the format info chunk
	BASS_StreamCreateFile/User/URL
* Ramping-in disabled for all but the XM format (and MODs in FT2 mode) in "normal" ramping mode
	BASS_MUSIC_RAMP (BASS_MusicLoad flag)
* RIFF "INFO" tag reading fix
	BASS_TAG_RIFF_INFO (BASS_ChannelGetTags type)
* OSX 10.7 compatibility fix
* x86_64 architecture support on OSX
* Delphi dynamic loading unit removed (now available as separate download)

2.4.7 - 30/11/2010
------------------
* Decode-to-position seeking
	BASS_POS_DECODETO (BASS_ChannelSetPosition flag)
* Support for RIFF "DISP" text tags
	BASS_TAG_RIFF_DISP (BASS_ChannelGetTags type)
* Support for APEv1 tags
	BASS_TAG_APE (BASS_ChannelGetTags type)
	BASS_TAG_APE_BINARY (BASS_ChannelGetTags type)
* 16384 sample FFT
	BASS_DATA_FFT16384 (BASS_ChannelGetData flag)
* Support for AIFF files larger than 4GB
	BASS_StreamCreateFile/User/URL
* Acceptance of self-signed and invalid certificates
	BASS_StreamCreateURL
* Internet stream reading timeout
	BASS_CONFIG_NET_READTIMEOUT (BASS_SetConfig option)
* Support for removing a DSP within its DSPPROC
	BASS_ChannelRemoveDSP
* Buffer bypassing for lower latency output (not available on Windows)
	BASS_ATTRIB_NOBUFFER (BASS_ChannelSetAttribute option)
	SYNTH example added (OSX)
* A "Default" device that follows default device changes on Windows 7
	BASS_CONFIG_DEV_DEFAULT (BASS_SetConfig option)
* Xcode examples project added for OSX
* Documentation updated with platform-specific info for iOS/Linux/WinCE

2.4.6 - 17/6/2010
-----------------
* Support for APEv2 tags, including binary
	BASS_TAG_APE (BASS_ChannelGetTags type)
	BASS_TAG_APE_BINARY (BASS_ChannelGetTags type)
* Unicode/IDN URL support
	BASS_StreamCreateURL
* Decoding position retrieval
	BASS_POS_DECODE (BASS_ChannelGetPosition flag)
* Support for OGG 6/7.1 channel configs
	BASS_StreamCreateFile/User/URL
* Syncing upon attribute changes
	BASS_SYNC_ATTRIB (BASS_ChannelSetSync type)
* Support for setting syncs within mixtime SYNCPROCs
	BASS_ChannelSetSync
* Support for setting additional DSP/FX within DSPPROCs
	BASS_ChannelSetDSP/FX
* DC bias removal in FFT processing
	BASS_DATA_FFT_REMOVEDC (BASS_ChannelGetData flag)
* Fix for incorrect (2x) return value when getting FFT data from a 16-bit channel
	BASS_ChannelGetData
* Fix for incorrect (STOPPED instead of PAUSED) channel status following BASS_Pause
	BASS_ChannelIsActive

2.4.5 - 18/12/2009
------------------
* Support for little-endian AIFF files
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad
* Support for 64-bit floating-point WAVE/AIFF files
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad
* Input volume retrieval failure results in a reading of -1 instead of 1
	BASS_RecordGetInput
	RECTEST example updated
* Input volume support on OSX
	BASS_RecordSetInput
	BASS_RecordGetInput
	RECTEST example updated
* Fix for deferred input settings on Vista
	BASS_RecordSetInput
* Windows MP3 codec given preference over other installed MP3 codecs (MP3-FREE version)
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad

2.4.4 - 13/10/2009
------------------
* RIFF/BWF "radio traffic" tag retrieval
	BASS_TAG_RIFF_CART (BASS_ChannelGetTags type)
	TAG_CART structure
* Support for ID3v2 tags in RIFF/WAVE/AIFF files ("ID3 " chunk)
	BASS_TAG_ID3V2 (BASS_ChannelGetTags type)
* Pushed fractional samples are refused rather than discarded
	BASS_StreamPutData
* DX8 effect emulation on OSX
	BASS_FX_DX8_CHORUS/DISTORTION/ECHO/FLANGER/PARAMEQ/REVERB (BASS_ChannelSetFX types)
	FXTEST example added
* UTF-16 support on OSX
	BASS_UNICODE (BASS_StreamCreateFile/SampleLoad/MusicLoad/PluginLoad flag)

2.4.3 - 8/5/2009
----------------
* MOD order list retrieval
	BASS_TAG_MUSIC_ORDERS (BASS_ChannelGetTags type)
* Support for ID3v2 tags in RIFF/WAVE files ("id3 " chunk)
	BASS_TAG_ID3V2 (BASS_ChannelGetTags type)
* Improved position reporting precision on Vista
	BASS_ChannelGetPosition
* Length retrieval when streaming in blocks (BASS_STREAM_BLOCK)
	BASS_ChannelGetLength
* Support for CoreAudio codecs on OSX
	BASS_StreamCreateFile/User
	BASS_SampleLoad
	BASS_TAG_CA_CODEC (BASS_ChannelGetTags type)
	TAG_CA_CODEC structure
* 3D algorithm option support on OSX
	BASS_CONFIG_3DALGORITHM (BASS_SetConfig option)

2.4.2 - 18/9/2008
-----------------
* RF64 support
	BASS_StreamCreateFile/User
* RIFF/BWF "Broadcast Audio Extension" tag retrieval
	BASS_TAG_RIFF_BEXT (BASS_ChannelGetTags type)
	TAG_BEXT structure
* ID3v1 tag structure
	TAG_ID3 structure
* Multiple simultaneous recordings per device on Vista & OSX (as on XP)
	BASS_RecordStart
* DX8 effect parameter defaults updated/corrected in documentation

2.4 - 2/4/2008
--------------
* "Push" streaming
	STREAMPROC_PUSH (BASS_StreamCreate option)
	BASS_StreamPutData
	LIVEFX and MULTI examples updated
* "Push" buffered file streaming
	STREAMFILE_BUFFERPUSH (BASS_StreamCreateFileUser system)
	BASS_StreamPutFileData
* STREAMFILEPROC replaced by table of callbacks for each file operation
	BASS_FILEPROCS (FILECLOSEPROC/FILELENPROC/FILEREADPROC/FILESEEKPROC)
	STREAMFILEPROC *removed*
* 64-bit file positioning
	BASS_SampleLoad
	BASS_MusicLoad
	BASS_StreamCreateFile
	BASS_StreamGetFilePosition
* File buffer level retrieval
	BASS_FILEPOS_BUFFER (BASS_StreamGetFilePosition mode)
* Sinc interpolated MOD music mixing
	BASS_MUSIC_SINCINTER (BASS_MusicLoad flag)
* MO3 2.4 support
	BASS_MusicLoad
* MOD orders positioning incorporated into channel functions
	BASS_ChannelGetLength
	BASS_ChannelSetPosition
	BASS_ChannelGetPosition
	BASS_MusicGetOrderPosition *removed*
	BASS_MusicGetOrders *removed*
* Channel attribute functions consolidated
	BASS_ChannelSetAttribute
	BASS_ChannelGetAttribute
	BASS_ChannelSlideAttribute
	BASS_ChannelIsSliding
	BASS_ChannelSetAttributes *removed*
	BASS_ChannelGetAttributes *removed*
	BASS_ChannelSlideAttributes *removed*
	BASS_ChannelSetEAXMix *removed*
	BASS_ChannelGetEAXMix *removed*
	BASS_MusicSetAttribute *removed*
	BASS_MusicGetAttribute *removed*
* Floating-point volume and panning
	BASS_SetVolume
	BASS_GetVolume
	BASS_RecordSetInput
	BASS_RecordGetInput
	BASS_ATTRIB_PAN/VOL (BASS_ChannelGet/Set/SlideAttribute options)
	BASS_ATTRIB_MUSIC_VOL_CHAN/INST (BASS_ChannelGet/Set/SlideAttribute options)
	BASS_SAMPLE volume/pan/outvol members
	BASS_CONFIG_MAXVOL *removed*
	BASSTEST and RECTEST examples updated
* Output device volume control on Vista (as on other OS)
	BASS_SetVolume
	BASS_GetVolume
* Multiple update threads
	BASS_CONFIG_UPDATETHREADS (BASS_SetConfig option)
	BASSTEST example updated
* Global volume range increased to 10000
	BASS_CONFIG_GVOL_SAMPLE/STREAM/MUSIC (BASS_SetConfig options)
	BASSTEST example updated
* Setting and retrieving of a sample's data
	BASS_SampleSetData
	BASS_SampleGetData
	BASS_SampleCreate
	BASS_SampleCreateDone *removed*
* Channel flag setting mask
	BASS_ChannelFlags
	BASS_ChannelSetFlags *removed*
	SPEAKERS example updated
* 256 sample FFT
	BASS_DATA_FFT256 (BASS_ChannelGetData flag)
* Channel locking to prevent access by other threads
	BASS_ChannelLock
* Manual channel buffer updating
	BASS_ChannelUpdate
	BASS_ChannelPreBuf *removed*
* Configurable manual update length
	BASS_Update
* Extended device information retrieval and detection of new/removed devices
	BASS_GetDeviceInfo
	BASS_RecordGetDeviceInfo
	BASS_DEVICEINFO structure
	BASS_GetDeviceDescription *removed*
	BASS_RecordGetDeviceDescription *removed*
	BASS_INFO driver member *removed*
	BASS_RECORDINFO driver member *removed*
	MULTI example updated
* Default device change tracking on Windows (as on OSX)
	BASS_Init
	BASS_RecordInit
* Speaker detection from Windows control panel
	BASS_DEVICE_CPSPEAKERS (BASS_Init flag)
* Channel automatically stopped & resumed for DX8 effects
	BASS_ChannelSetFX
	BASS_ChannelRemoveFX
* "double" precision position conversion
	BASS_ChannelBytes2Seconds
	BASS_ChannelSeconds2Bytes
* Separate config functions for pointers
	BASS_SetConfigPtr
	BASS_GetConfigPtr
	BASS_CONFIG_NET_AGENT/PROXY (BASS_SetConfigPtr options)
* Configurable file format verification length
	BASS_CONFIG_VERIFY (BASS_SetConfig option)
* Stream filename retrieval
	BASS_CHANNELINFO file member
* Channel sample retrieval
	BASS_CHANNELINFO sample member
* META syncs no longer receive metadata in the "data" parameter
	BASS_SYNC_META (BASS_ChannelSetSync type)
* Separate sync for OGG logical bitstream changes (instead of BASS_SYNC_META)
	BASS_SYNC_OGG_CHANGE (BASS_ChannelSetSync type)
	NETRADIO example updated (C version)
* Message syncing removed (use PostMessage instead)
	BASS_SYNC_MESSAGE (BASS_ChannelSetSync flag) *removed*
* Data retrieval from stopped/paused channels
	BASS_ChannelGetData
* Callback "user" parameters changed to pointers
	BASS_StreamCreate / STREAMPROC
	BASS_StreamCreateFileUser
	BASS_StreamCreateURL / DOWNLOADPROC
	BASS_RecordStart / RECORDPROC
	BASS_ChannelSetDSP / DSPPROC
	BASS_ChannelSetSync / SYNCPROC

2.3.0.3 - 30/7/2007
-------------------
* FX state resetting
	BASS_FXReset
* PLS/M3U playlist URL processing
	BASS_CONFIG_NET_PLAYLIST (BASS_SetConfig option)
	NETRADIO example updated
* Internet stream connection status retrieval
	BASS_FILEPOS_CONNECTED (BASS_StreamGetFilePosition mode)
* Lyrics3v2 tags
	BASS_TAG_LYRICS3 (BASS_ChannelGetTags type)
* IT virtual channel configuration
	BASS_CONFIG_MUSIC_VIRTUAL (BASS_SetConfig option)
* Accurate speaker detection on Vista
	BASS_INFO speakers member
* Device output/input rate retrieval on Vista
	BASS_INFO freq member
	BASS_RECORDINFO freq member
* Syncing upon position changes
	BASS_SYNC_SETPOS (BASS_ChannelSetSync type)
* Improved stall handling
	BASS_SYNC_STALL
* Invalid decoding channel flags produce an error instead of being ignored
	BASS_StreamCreate/File/User/URL
	BASS_MusicLoad

2.3.0.2 - 22/1/2007
-------------------
* Retrieval of a sample's existing channel handles
	BASS_SampleGetChannels
* 8192 sample FFT
	BASS_DATA_FFT8192 (BASS_ChannelGetData flag)
* Adjustable recording buffer
	BASS_CONFIG_REC_BUFFER (BASS_SetConfig option)
* Stopping decoding channels before the end
	BASS_ChannelStop
* Sample channels created paused to prevent overriding before playback
	BASS_SampleGetChannel
* Separate "MP3-FREE" version using the OS's MP3 decoder
	BASS_CONFIG_MP3_CODEC *removed*

2.3.0.1 - 12/6/2006
-------------------
* Ability to move a channel to another device
	BASS_ChannelSetDevice
	MULTI example updated
* Support for ID3v2.4 tags at end of file
	BASS_TAG_ID3V2 (BASS_ChannelGetTags type)

2.3 - 21/5/2006
---------------
* MOD message/instrument/sample text retrieval, merged with stream tag retrieval function
	BASS_ChannelGetTags
	BASS_TAG_MUSIC_NAME/MESSAGE/INST/SAMPLE (BASS_ChannelGetTags types)
	BASS_MusicGetName *removed*
	BASS_StreamGetTags *removed*
* Plugin information retrieval
	BASS_PluginGetInfo
	BASS_PLUGININFO structure
	BASS_PLUGINFORM structure
	BASS_CHANNELINFO plugin member
	PLUGINS example updated
* RIFF/WAVE "INFO" tag retrieval
	BASS_TAG_RIFF_INFO (BASS_StreamGetTags type)
* More specific WAVE format information
	BASS_CTYPE_STREAM_WAV_PCM/FLOAT (channel types)
	BASS_CTYPE_STREAM_WAV (channel type flag)
* Proxy server configuration
	BASS_CONFIG_NET_PROXY (BASS_SetConfig option)
	BASS_CONFIG_NET_NOPROXY *removed*
	NETRADIO example updated
* Passive FTP mode
	BASS_CONFIG_NET_PASSIVE (BASS_SetConfig option)
* Agent changes take immediate effect
	BASS_CONFIG_NET_AGENT (BASS_SetConfig option)
* Minimum time gap between creating new sample channels
	BASS_SAMPLE mingap member
	BASS_SampleGetChannel
* Support for Unicode plugin filenames
	BASS_PluginLoad
* Device output/input rate retrieval (MacOSX only)
	BASS_INFO freq member
	BASS_RECORDINFO freq member
* Extended version info (minor revision)
	BASS_GetVersion
* Unsupported codec error code
	BASS_ERROR_CODEC
* Optional use of the Windows MP3 codec
	BASS_CONFIG_MP3_CODEC (BASS_SetConfig option)
* 3D support for MacOSX
	BASS_Set3DFactors
	BASS_Get3DFactors
	BASS_Set3DPosition
	BASS_Get3DPosition
	BASS_Apply3D
	BASS_ChannelSet3DAttributes
	BASS_ChannelGet3DAttributes
	BASS_ChannelSet3DPosition
	BASS_ChannelGet3DPosition
	3DTEST example added (Win32 example also updated)
* VB version of SYNTH example added

2.2 - 2/10/2005
---------------
* Add-on plugin system
	BASS_PluginLoad
	BASS_PluginFree
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad
	PLUGINS example added
* MOD position & syncing in bytes
	BASS_ChannelSetPosition
	BASS_ChannelGetPosition
	BASS_MusicGetOrderPosition added for orders position
	BASS_SYNC_MUSICPOS added for orders syncing
	MAKEMUSICPOS macro/define
	CUSTLOOP example updated
* Stream/MOD "length" functions combined (also works with samples), new MOD orders length function
	BASS_ChannelGetLength
	BASS_MusicGetOrders
	BASS_StreamGetLength *removed*
	BASS_MusicGetLength *removed*
* Support for AIFF files
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad
* Support for 24 and 32-bit (integer) WAV files
	BASS_StreamCreateFile/User/URL
	BASS_SampleLoad
* WAV files are no longer converted to the "device" resolution
	BASS_StreamCreateFile/User/URL
* Recording master control
	BASS_RecordGetInput
	BASS_RecordSetInput
* Adjustable pre-buffering
	BASS_ChannelPreBuf
* Floating-point data retrieval
	BASS_DATA_FLOAT (BASS_ChannelGetData flag)
* Support for floating-point samples
	BASS_SampleLoad
	BASS_SampleCreate
* Multi-channel samples
	BASS_SampleLoad/Create
	BASS_SAMPLE chans member
* Sample lengths given in bytes
	BASS_SampleCreate
	BASS_SAMPLE length member
* MOD music 8-bit resolution option
	BASS_MusicLoad
* OGG vendor tag retrieval
	BASS_TAG_VENDOR (BASS_StreamGetTags type)
* Configurable "User-Agent" header for internet streams
	BASS_CONFIG_NET_AGENT (BASS_SetConfig option)
* Shoutcast metadata is now requested automatically
	BASS_STREAM_META flag *removed*
* Download callbacks receive all data from start of file/stream (including any non-audio data)
	DOWNLOADPROC
* Length when streaming in blocks is unavailable (BASS_ERROR_NOTAVAIL, not just 0)
	BASS_ChannelGetLength
* Support for growing custom file streams
	BASS_FILE_LEN (STREAMFILEPROC action)
* Query file action removed
	BASS_FILE_QUERY *removed*
* Recording channel syncing
	BASS_ChannelSetSync
* Info structure "size" members removed
	BASS_INFO structure
	BASS_RECORDINFO structure
* Little bit of flag reshuffling
	BASS_MP3_SETPOS renamed to BASS_STREAM_PRESCAN
	BASS_MUSIC_CALCLEN value changed and renamed to BASS_MUSIC_PRESCAN
	BASS_MUSIC_POSRESET value changed
* Add-on API enhancements
* MacOSX port introduced

2.1 - 28/11/2004
----------------
* Improved "mixtime" sync system, allowing custom looping
	SYNCPROC
	CUSTLOOP example added
* FX can now be in the DSP chain, so can be prioritized and applied in floating-point
	BASS_ChannelSetFX
	BASS_CONFIG_FLOATDSP (BASS_SetConfig option)
* Ability to set channel flags (eg. looping) independent of playback
	BASS_ChannelSetFlags
	SPEAKERS example updated
* Stream/MOD "play" and channel "resume" functions combined
	BASS_ChannelPlay
	BASS_StreamPlay *removed*
	BASS_MusicPlay *removed*
	BASS_MusicPlayEx *removed*
	BASS_ChannelResume *removed*
* Stream/MOD pre-buffering functions combined
	BASS_ChannelPreBuf
	BASS_StreamPreBuf *removed*
	BASS_MusicPreBuf *removed*
* MOD attribute functions combined, with added BPM/speed/globalvolume options
	BASS_MusicSetAttribute
	BASS_MusicGetAttribute
	BASS_MUSIC_ATTRIB_AMPLIFY
	BASS_MUSIC_ATTRIB_PANSEP
	BASS_MUSIC_ATTRIB_PSCALER
	BASS_MUSIC_ATTRIB_BPM
	BASS_MUSIC_ATTRIB_SPEED
	BASS_MUSIC_ATTRIB_VOL_GLOBAL
	BASS_MUSIC_ATTRIB_VOL_CHAN
	BASS_MUSIC_ATTRIB_VOL_INST
	BASS_MusicSetAmplify *removed*
	BASS_MusicSetPanSep *removed*
	BASS_MusicSetPositionScaler *removed*
	BASS_MusicSetVolume *removed*
	BASS_MusicGetVolume *removed*
* Flag to reset bpm/etc as well as notes when seeking in MOD musics
	BASS_MUSIC_POSRESETEX (BASS_MusicLoad & BASS_ChannelSetFlags flag)
* More flexible and concise sample channel creation system
	BASS_SampleGetChannel
	BASS_SamplePlay *removed*
	BASS_SamplePlayEx *removed*
	BASS_SamplePlay3D *removed*
	BASS_SamplePlay3DEx *removed*
* Support for up to 30 speakers
	BASS_SPEAKER_N macro/define
* More precise level measurement
	BASS_ChannelGetLevel
* Level can now be retrieved from decoding channels
	BASS_ChannelGetLevel
* Retrieval of a sample/channel's original sample resolution
	BASS_SAMPLE origres member
	BASS_CHANNELINFO origres member
* Support for streaming WAV files in "blocks"
	BASS_StreamCreateURL
	BASS_StreamCreateFileUser
* Status info (HTTP/ICY tags) available during connection to server
	BASS_STREAM_STATUS (BASS_StreamCreateURL flag)
	DOWNLOADPROC
	NETRADIO example updated (Delphi version also added)
* Adjustable internet stream pre-buffering
	BASS_CONFIG_NET_PREBUF (BASS_SetConfig option)
* Option to bypass proxy server
	BASS_CONFIG_NET_NOPROXY (BASS_SetConfig option)
* Option whether to allow channels to be played after BASS_Pause
	BASS_CONFIG_PAUSE_NOPLAY (BASS_SetConfig option)
* Recording channel count now a separate parameter
	BASS_RecordStart (chans parameter)
* Synchronizer for when a channel is freed
	BASS_SYNC_FREE (BASS_ChannelSetSync type)
* Data start file position retrieval
	BASS_FILEPOS_START (BASS_StreamGetFilePosition mode)
* Performance improvements
	MP2 decoding ~20% faster
	MP3/MP1 decoding & FFT processing all up to 10% faster
	OGG decoding ~3% faster
* C/C++ examples reorganised, with makefiles & VC++ projects
* Add-on API enhancements
* More DLL shrinkage :)

2.0 - 31/10/2003
----------------
* Multiple output device support
	BASS_Init (device number changes)
	BASS_SetDevice
	BASS_GetDevice
	BASS_ChannelGetDevice
	MULTI example updated (VB version also added)
* Multiple recording device support
	BASS_RecordSetDevice
	BASS_RecordGetDevice
	BASS_RecordStart
	BASS_ChannelGetDevice
	HRECORD handle
	RECORDPROC (handle parameter)
* Recording with DSP/FX
	BASS_ChannelSetDSP
	BASS_ChannelSetFX
* Recording position retrieval
	BASS_ChannelGetPosition
* Start recording paused
	BASS_RECORD_PAUSE (BASS_RecordStart flag)
* Multi-channel streams, inc. support for multichannel OGG & WAV files
	BASS_StreamCreate
	BASS_StreamCreateFile/User/URL
* FFT for individual channels, inc. multi-channel streams
	BASS_DATA_FFT_INDIVIDUAL (BASS_ChannelGetData flag)
	BASS_DATA_FFT512S/1024S/2048S/4096S *removed*
* DSP prioritizing
	BASS_ChannelSetDSP
	DSPTEST example updated
* Seeking in internet streamed files
	BASS_ChannelSetPosition
* Enhanced custom file stream systems
	BASS_StreamCreateFileUser
	BASS_FILE_SEEK (STREAMFILEPROC action)
	BASS_STREAM_FILEPROC flag *removed*
* Enhanced custom stream system with automatic stalling/resuming
	STREAMPROC
	BASS_STREAMPROC_END (STREAMPROC flag)
* Synchronizer for stalled/resumed playback 
	BASS_SYNC_STALL (BASS_ChannelSetSync type)
* Synchronizer for completed download
	BASS_SYNC_DOWNLOAD (BASS_ChannelSetSync type)
* End sync support for custom streams
	BASS_SYNC_END (BASS_ChannelSetSync type)
* Synchronizer support for decoding channels
	BASS_ChannelSetSync
* Unified configuration function
	BASS_SetConfig
	BASS_GetConfig
	BASS_SetBufferLength *removed*
	BASS_SetNetConfig *removed*
	BASS_SetGlobalVolumes *removed*
	BASS_GetGlobalVolumes *removed*
	BASS_SetLogCurves *removed*
	BASS_Set3DAlgorithm *removed*
	BASS_DEVICE_FLOATDSP flag *removed*
* Internet stream saving to disk replaced by more flexible callback
	BASS_StreamCreateURL
	DOWNLOADPROC
	VB NETRADIO example updated
* Buffer length retrieval when "streaming in blocks"
	BASS_FILEPOS_END (BASS_StreamGetFilePosition mode)
* Individual sample rate setting for MOD musics
	BASS_MusicLoad
* Channel type and default sample rate retrieval
	BASS_ChannelGetInfo (replaces BASS_ChannelGetFlags)
	BASS_CHANNELINFO
* MOD music flag retrieval
	BASS_CHANNELINFO flags member
* Adjustable instrument volumes in MOD musics
	BASS_MusicSetVolume (replaces BASS_MusicSetChannelVol)
	BASS_MusicGetVolume (replaces BASS_MusicGetChannelVol)
* Automatically free a MOD music when it stops or ends
	BASS_MUSIC_AUTOFREE (BASS_MusicLoad flag)
* Class GUID added to initialization parameters
	BASS_Init
	BASS_SetCLSID *removed*
* Update period adjustable at any time
	BASS_CONFIG_UPDATEPERIOD (BASS_SetConfig option)
	BASS_DEVICE_NOTHREAD flag *removed*
* Customizable maximum volume setting
	BASS_CONFIG_MAXVOL (BASS_SetConfig option)
	BASS_DEVICE_VOL1000 flag *removed*
* Device volume is now always left as it is during init/freeing
	BASS_DEVICE_LEAVEVOL flag *removed*
* Device driver name retrieval
	BASS_INFO driver member
	BASS_RECORDINFO driver member
* Error codes are local to the current thread
	BASS_ErrorGetCode
* Performance improvements
	MP2 decoding 15-20% faster
	MP3 decoding ~5% faster
* Built-in CD functions removed (replaced in BASSCD)
	BASS_CDDoor *removed*
	BASS_CDFree *removed*
	BASS_CDGetID *removed*
	BASS_CDGetTrackLength *removed*
	BASS_CDGetTracks *removed*
	BASS_CDInDrive *removed*
	BASS_CDInit *removed*
	BASS_CDPlay *removed*
* Force channels to use software mixing
	BASS_SAMPLE_SOFTWARE (BASS_StreamCreate/File/User/URL & BASS_MusicLoad flag)
* Support for high-pass filter and forward/reverse (S9E/F) IT/MPT effects
* BASS_MUSIC flags rearranged to mirror BASS_SAMPLE/STREAM counterparts
* Output automatically started during initialization
* BASS_ChannelGetData once again accepts any "length" param
* All function comments have been removed from the API headers to avoid
  outdated/incorrect information - the documentation should be used.
* TMT Pascal API removed from main distribution - now available on the website
* A few more 'K' knocked off the DLL size :)

1.8a - 18/6/2003
----------------
* Tweaks 'n' fixes, including...
	Fixed seeking bug on 32-bit OGG streams
	Fixed seeking on a decoding channel after it has reached the end
	Low FPU precision (eg. when using Direct3D) issue addressed
	Improved speakers (BASS_INFO) detection
	BASS_ChannelSeconds2Bytes return value is rounded down to nearest sample
	BASS_ChannelGetData "length" param must equal a whole number of samples
	Slide syncs are triggered by "-2" volume slides on "autofree" streams
* Support for Unicode filenames
	BASS_UNICODE (BASS_SampleLoad/BASS_StreamCreateFile/BASS_MusicLoad flag)
* 4096 sample FFT
	BASS_DATA_FFT4096/S (BASS_ChannelGetData flags)
* Another 'K' knocked off the DLL size

1.8 - 9/3/2003
--------------
* 32-bit floating-point channels
	BASS_SAMPLE_FLOAT (BASS_StreamCreate/File/URL flag)
	BASS_MUSIC_FLOAT (BASS_MusicLoad flag)
	BASS_SAMPLE_FLOAT (BASS_RecordStart flag)
	BASS_DEVICE_FLOATDSP (BASS_Init flag)
	DSPTEST example updated
* Support for 32-bit floating-point (type 3) WAV files
	BASS_StreamCreateFile/URL
	BASS_SampleLoad
* Channel speaker assignment
	BASS_SPEAKER_FRONT (BASS_MusicLoad/BASS_StreamCreate/File/URL flag)
	BASS_SPEAKER_REAR                       "
	BASS_SPEAKER_CENLFE                     "
	BASS_SPEAKER_REAR2                      "
	BASS_SPEAKER_FRONTLEFT                  "
	BASS_SPEAKER_FRONTRIGHT                 "
	BASS_SPEAKER_REARLEFT                   "
	BASS_SPEAKER_REARRIGHT                  "
	BASS_SPEAKER_CENTER                     "
	BASS_SPEAKER_LFE                        "
	BASS_SPEAKER_REAR2LEFT                  "
	BASS_SPEAKER_REAR2RIGHT                 "
	BASS_INFO speakers member
	BASS_DEVICE_SPEAKERS (BASS_Init flag)
	4SPEAKER example replaced by SPEAKERS example
* Recording input type retrieval
	BASS_INPUT_TYPE_xxx (BASS_RecordGetInput)
	RECTEST example updated
* Non-interpolated MOD mixing
	BASS_MUSIC_NONINTER (BASS_MusicLoad/PlayEx flag)
* Performance improvements
	FFT up to 100% faster!
	MP3(MPEG2/2.5) decoding up to 60% faster
	MMX mixers 5-10% faster
	MP3(MPEG1)/MP2/MP1/OGG decoding all ~5% faster
* Optional disabling of FFT windowing
	BASS_DATA_FFT_NOWINDOW (BASS_ChannelGetData flag)
* BASS_ERROR_FILEFORM - error code to distinguish between file and sample format
	BASS_MusicLoad
	BASS_SampleLoad
	BASS_StreamCreate/File/URL
* BASS_StreamGetFilePosition mode flags added
	BASS_FILEPOS_DECODE/DOWNLOAD/END
* DirectSound 9 detection
	BASS_INFO dsver member
* Initialization flags retrieval
	BASS_INFO initflags member
* Half-rate MP3 playback option removed
	BASS_MP3_HALFRATE flag *removed*
* New internal "plugin" system - BASSWMA is further integrated as a result
* Improved documentation - integrated with BASSWMA, search option added
* VB version of DSPTEST example added
* Delphi RECORDTEST example added
* Guess what... reduced DLL size again :)

1.7 - 27/10/2002
----------------
* New alternative DX8 (DMO) effects implementation
	BASS_StreamCreate/File/URL
	BASS_MusicLoad
	BASS_ChannelSetFX
	SYNTH example updated
* User file streaming
	BASS_STREAM_FILEPROC (BASS_StreamCreate flag)
	STREAMFILEPROC
* DSP & FX support for decoding channels
	BASS_ChannelSetDSP
	BASS_ChannelSetFX
* Support for DX8 (DMO) effects in IT/XM/MO3 files
	BASS_MusicLoad
* Support for chained OGG streaming (and syncing)
	BASS_StreamCreateURL
	BASS_SYNC_META (BASS_ChannelSetSync type)
* Attribute (volume/frequency/pan) sliding
	BASS_ChannelSlideAttributes
	BASS_ChannelIsSliding
	BASS_SYNC_SLIDE (BASS_ChannelSetSync type)
* Recording without a callback function
	BASS_RecordStart
	LIVEFX example added
* Query a channel's buffered data
	BASS_DATA_AVAILABLE (BASS_ChannelGetData flag)
* Discard data from the recording buffer
	BASS_ChannelGetData
* Adjustable internet stream config (timeout/buffer lengths)
	BASS_SetNetConfig
* Recommended minimum buffer length
	BASS_INFO minbuf member
* MOD music flags adjustment without changing playback position
	BASS_MusicPlayEx
	PLAYER (MASM) example updated
* More functions are now useable in MOD music "mixtime" syncs
	SYNCPROC
	BASS_ChannelSetPosition
	BASS_MusicPlayEx
	BASS_MusicSetAmplify
	BASS_MusicSetPanSep
* Maximum buffer length increased to 5 seconds
	BASS_SetBufferLength
* Support for extended filter range in IT files
	BASS_MusicLoad
* Speedier MOD music file verification
	BASS_MusicLoad
* Delphi 3DTEST example fixed
* Magically reduced DLL size again :)

1.6a - 25/8/2002
----------------
* OGG support updated to 1.0
* Stereo FFT
	BASS_DATA_FFT512S/1024S/2048S (BASS_ChannelGetData flags)
* Support for "Invert Loop" (EFx) MOD effect
* Reduced DLL size
* New Delphi examples
	WRITEWAV - WAVE writer example
	SAMPLEVIS - Visualisation example

1.6 - 13/6/2002
---------------
* 64-bit stream lengths and positions
	BASS_StreamGetLength
	BASS_ChannelBytes2Seconds
	BASS_ChannelSeconds2Bytes
	BASS_ChannelGetPosition
	BASS_ChannelSetPosition
	BASS_ChannelSetSync
* Recording input selection
	BASS_RECORDINFO inputs/singlein members
	BASS_RecordGetInputName
	BASS_RecordGetInput
	BASS_RecordSetInput
* Adjustable recording update period
	BASS_RecordStart
* Load OGG files as samples
	BASS_SampleLoad
* CD drive door opening & closing
	BASS_CDDoor
* CDDB2 ID retrieval
	BASS_CDID_CDDB2 (BASS_CDGetID flag)
* Streaming beyond initial file length
	BASS_StreamCreateFile
* Recording position bytes<->seconds translation
	BASS_ChannelBytes2Seconds
	BASS_ChannelSeconds2Bytes
* Improved multi-threaded support (play from any thread)
	BASS_MusicPlay/Ex
	BASS_SamplePlay/3D/Ex
	BASS_StreamPlay
	BASS_DEVICE_NOSYNC flag *removed*
* Paused channel status
	BASS_ACTIVE_PAUSED (BASS_ChannelIsActive)
* Integrated WMA stream freeing
	BASS_StreamFree
	BASS_Free
* Pin-point accurate OGG seeking without BASS_MP3_SETPOS flag
* Win2k DS buffer bug fix

1.5a - 14/4/2002
----------------
* NT4 fix (also enables "nosound" device without DX installed)
	BASS_ERROR_DX error code
* MOD music loading without the samples
	BASS_MUSIC_NOSAMPLE (BASS_MusicLoad flag)
* Custom decoding channels
	BASS_STREAM_DECODE (BASS_StreamCreate flag)
* 5 second HTTP connection timeout
	BASS_ERROR_TIMEOUT (BASS_StreamCreateURL error code)

1.5 - 31/3/2002
---------------
* Improved performance
	MMX mixers lot faster (over 50% faster in some cases!)
	OGG decoding 15-20% faster
	MP3 decoding 5-10% faster
* Recording
	BASS_RecordGetDeviceDescription
	BASS_RecordInit
	BASS_RecordFree
	BASS_RecordGetInfo
	BASS_RecordStart
* OGG support built-in (OGG/VORBIS DLLs not required)
	BASS_DEVICE_OGG flag *removed*
* MOD music seeking in seconds
	BASS_MusicPlayEx
	BASS_ChannelSetPosition
* Shoutcast metadata retrieval
	BASS_STREAM_META (BASS_StreamCreateURL flag)
	BASS_TAG_META (BASS_StreamGetTags type)
	BASS_SYNC_META (BASS_ChannelSetSync type)
* 1000 volume levels
	BASS_DEVICE_VOL1000 (BASS_Init/CDInit flag)
* CDDB ID retrieval
	BASS_CDID_CDDB (BASS_CDGetID flag)
* Leave the CD volume as it is during init/closing
	BASS_DEVICE_LEAVEVOL (BASS_CDInit flag)
* FFT enabled on decoding channels
	BASS_ChannelGetData
* Left level duplicated on right for mono channels
	BASS_ChannelGetLevel
* Improved MPEG length estimation without BASS_MP3_SETPOS flag
	BASS_StreamGetLength
* Support for Modplug/ADPCM compressed files
	BASS_MusicLoad
* Device description function parameter change
	BASS_GetDeviceDescription
* MASM API

1.4 - 30/1/2002
---------------
* Channel decoding without playback
	BASS_MUSIC_DECODE (BASS_MusicLoad flag)
	BASS_STREAM_DECODE (BASS_StreamCreateFile/URL flag)
	BASS_ChannelGetData
* Windows message sync callbacks
	BASS_SYNC_MESSAGE (BASS_ChannelSetSync flag)
* Adjustable channel volumes in MOD musics
	BASS_MusicSetChannelVol
	BASS_MusicGetChannelVol
* Customizable DirectSound initialization object
	BASS_SetCLSID
* Retrieve HMUSIC/HSTREAM/HCHANNEL IDirectSoundBuffer interfaces
	BASS_GetDSoundObject
* A3D functions removed (use BASS_SetCLSID/BASS_GetDSoundObject to access A3D)
	BASS_DEVICE_A3D (BASS_Init flag)
	BASS_SetA3DResManager
	BASS_GetA3DResManager
	BASS_SetA3DHFAbsorbtion
	BASS_GetA3DHFAbsorbtion
* Callback functions now work in VB6
	DSPPROC
	STREAMPROC
	SYNCPROC
* Improved PCM WAVE streaming performance
	BASS_StreamCreateFile
	BASS_StreamCreateURL
* OGG modules updated to RC3
* Stereo sample support in MO3 format
* MO3 encoder now distributed separately from BASS

1.3 - 17/11/2001
----------------
* Manual buffer updating
	BASS_DEVICE_NOTHREAD (BASS_Init flag)
	BASS_Update
* Adjustable buffer update period (allows small buffer sizes)
	BASS_Init
* Output device latency retrieval
	BASS_INFO latency member
* MPEG/OGG seeking without BASS_MP3_SETPOS flag
	BASS_ChannelSetPosition
* Internet file streaming from offsets
	BASS_StreamCreateURL
* File stream tag/comment retrieval (ID3/ID3v2/OGG/HTTP/ICY tags)
	BASS_StreamGetTags
* Byte<->time position translation
	BASS_ChannelBytes2Seconds
	BASS_ChannelSeconds2Bytes
* UMX (Unreal/Tournament music package) format support
	BASS_MusicLoad
* S3M/IT sync fx changed to S2x (S0x conflicted with S00)
	BASS_SYNC_MUSICFX
* Stereo sample support in IT/XM/S3M formats
* MO3: OGG compression supported

1.2 - 25/9/2001
---------------
* OGG (Ogg Vorbis) stream support
	BASS_DEVICE_OGG (BASS_Init flag)
	BASS_StreamCreateFile
	BASS_StreamCreateURL
* Channel linking (start/stop/pause/resume channels together)
	BASS_ChannelSetLink
	BASS_ChannelRemoveLink
* MOD music playback length calculation
	BASS_MUSIC_CALCLEN (BASS_MusicLoad flag)
	BASS_MusicGetLength
* Pre-buffering
	BASS_MusicPreBuf
	BASS_StreamPreBuf
* Samples with single simultaneous playbacks have same HSAMPLE/HCHANNEL handle
	BASS_SamplePlay/Ex
* Stopping a custom stream flushes its buffer contents
	BASS_ChannelStop

1.1a - 31/8/2001
----------------
* NT4 bug fixed
* XM Wxx effect syncing
	BASS_SYNC_MUSICFX
* MP3/2/1 rewinding without BASS_MP3_SETPOS
	BASS_ChannelSetPosition

1.1 - 11/8/2001
---------------
* DX8 (DMO) effects
	BASS_SAMPLE_FX (BASS_StreamCreate/File/URL flag)
	BASS_MUSIC_FX (BASS_MusicLoad flag)
	BASS_ChannelSetFX
	BASS_ChannelRemoveFX
	BASS_FXSetParameters
	BASS_FXGetParameters
	BASS_FXCHORUS structure
	BASS_FXCOMPRESSOR structure
	BASS_FXDISTORTION structure
	BASS_FXECHO structure
	BASS_FXFLANGER structure
	BASS_FXGARGLE structure
	BASS_FXI3DL2REVERB structure
	BASS_FXPARAMEQ structure
	BASS_FXREVERB structure
* Internet file streaming in blocks (inc. Shoutcast/Icecast stream support)
	BASS_STREAM_BLOCK (BASS_StreamCreateURL flag)
* 512/1024/2048 sample FFT
	BASS_DATA_FFT512/1024/2048 (BASS_ChannelGetData flags)
* CD identification
	BASS_CDGetID
* Improved DX version detection
	BASS_INFO dsver member

1.0 - 20/6/2001
---------------
* Load MP3/MP2/MP1 files as samples
	BASS_SampleLoad
* Internet file streaming from FTP servers
	BASS_StreamCreateURL
* Save a local copy of internet file streams
	BASS_StreamCreateURL
* Sample accurate file stream seeking
	BASS_ChannelSetPosition
	BASS_StreamGetBlockLength *removed*
* Stream position synchronizer
	BASS_SYNC_POS
* Increased synchronizer precision
* Improved MPEG file detection and error detection
* Stop MOD musics on a backwards jump effect
	BASS_MUSIC_STOPBACK (BASS_MusicLoad/PlayEx flag)
* Leave the volume as it is during closing (as well as initialization)
	BASS_DEVICE_LEAVEVOL (BASS_Init flag)
* Optional automatic use of foreground window handle during initialization
	BASS_Init
* Reduced DLL size
* VB API fixes

0.9 - 18/4/2001
---------------
* Internet file streaming
	BASS_StreamCreateURL
* MP1 & MP2 (MPEG layer 1 & 2) support
	BASS_StreamCreateFile/URL
* MPEG 2.5 support (12000/11025/8000hz sample rates)
	BASS_StreamCreateFile/URL
* Decoding/download/end file stream position retrieval
	BASS_StreamGetFilePosition
* XMPlay surround sound for MOD musics
	BASS_MUSIC_SURROUND (BASS_MusicLoad/PlayEx flag)
	BASS_MUSIC_SURROUND2 (BASS_MusicLoad/PlayEx flag)
* Restrict the download rate of internet file streams
	BASS_STREAM_RESTRATE (BASS_StreamCreateURL flag)
* Check if an internet file stream is stalled
	BASS_ChannelIsActive
* Automatically free a stream when it stops or ends
	BASS_STREAM_AUTOFREE (BASS_StreamCreate/File/URL flag)
* Leave the volume as it is during initialization
	BASS_DEVICE_LEAVEVOL (BASS_Init flag)
* Number of CD tracks retrieval
	BASS_CDGetTracks
* CD track length retrieval
	BASS_CDGetTrackLength
* Exact stream length set after whole file is streamed
	BASS_StreamGetLength
* TMT Pascal API and samples
* Dynamic-loading Delphi API

0.8a - 28/2/2000
----------------
* Updated Delphi API and samples

0.8 - 24/1/2000
---------------
* Improved MP3 performance on P2/K6 and above CPUs
* User DSP functions on streams and MOD musics
	BASS_ChannelSetDSP
	BASS_ChannelRemoveDSP
* DX7 voice allocation & management
	BASS_SAMPLE_VAM (BASS_SampleLoad/Create flag)
	BASS_VAM_xxx flags
	BASS_SAMPLE vam/priority members
* DX7 software 3D algorithm selection
	BASS_Set3DAlgorithm
* DirectSound interface retrieval
	BASS_GetDSoundObject
* Log/linear volume & panning curves
	BASS_SetLogCurves
* User data passed to callback functions
	STREAMPROC - BASS_StreamCreate
	SYNCPROC - BASS_ChannelSetSync
* New synchronizer
	BASS_SYNC_MUSICFX
* New synchronizer flag
	BASS_SYNC_MIXTIME
* Disable synchronizers option - saves a little CPU time
	BASS_DEVICE_NOSYNC (BASS_Init flag)
* Hi-res floating-point CPU usage monitoring
	BASS_GetCPU
* Wait for playback to start when playing a CD
	BASS_CDPlay
* DirectSound (dsound.dll) version retrieval
	BASS_INFO dsver member
* Removed volume sliding functions (they were fairly pointless)
	BASS_SlideVolume
	BASS_IsSliding
* MO3: read/write encoder settings
* MO3: remove inst/samp/message texts now optional
* MO3: LAME encoder settings

0.7 - 3/10/1999
---------------
* MO3 (MP3 compressed MODs)
* A3D functions
	BASS_DEVICE_A3D (BASS_Init flag)
	BASS_INFO a3d member
	BASS_SetA3DResManager
	BASS_GetA3DResManager
	BASS_SetA3DHFAbsorbtion
	BASS_GetA3DHFAbsorbtion
* Music/stream immediate sample data retrieval
	BASS_ChannelGetData
* File stream (WAV/MP3) length retrieval
	BASS_StreamGetLength
	BASS_StreamGetBlockLength
* File stream seeking
	BASS_ChannelSetPosition
* Mono MP3 option (lower CPU usage)
	BASS_StreamCreateFile
* Music length retrieval
	BASS_MusicGetLength
* Music name retrieval
	BASS_MusicGetName
* Stop notes when moving MOD music position
	BASS_MUSIC_POSRESET (BASS_MusicLoad/BASS_MusicPlayEx flag)
* BASS_ERROR_FREQ - invalid sample rate error code
	BASS_SampleCreate
	BASS_SamplePlayEx
	BASS_SamplePlay3DEx
	BASS_StreamCreate
	BASS_ChannelSetAttributes
* Delphi and VB APIs

0.6a - 26/7/1999
----------------
* Half rate MP3 option (lower CPU usage)
	BASS_MP3_HALFRATE
* Loading/streaming from file offsets
	BASS_MusicLoad
	BASS_SampleLoad
	BASS_StreamCreateFile
* Global music/sample/stream volume levels
	BASS_SetGlobalVolumes
	BASS_GetGlobalVolumes
* Other new function
	BASS_SampleStop
* New synchronizer
	BASS_SYNC_END
* New sample overrider
	BASS_SAMPLE_OVER_DIST
* LoadLibrary/GetProcAddress instructions and example

0.5 - 4/7/1999
--------------
* Documentation!
* File streaming (MP3 and WAV)
	BASS_StreamCreateFile
* Custom generated samples
	BASS_SampleCreate
	BASS_SampleCreateDone
* Other new function
	BASS_MusicSetPositionScaler
* Renamed function
	BASS_ChannelClearSync -> BASS_ChannelRemoveSync
* Alterations made to
	BASS_ChannelGetPosition
	BASS_SampleLoad
	BASS_StreamPlay

0.4 - 30/3/1999
---------------
* Compressed WAV samples support (using audio CODECs)
* Updated CD volume handling - now works with SB Live
* More linear channel volume/pan scales (were slightly off before)
* "No sound" device option
* 3D sound functions
	BASS_Set3DFactors
	BASS_Get3DFactors
	BASS_Set3DPosition
	BASS_Get3DPosition
	BASS_Apply3D
	BASS_SamplePlay3D
	BASS_SamplePlay3DEx
	BASS_ChannelSet3DAttributes
	BASS_ChannelGet3DAttributes
	BASS_ChannelSet3DPosition
	BASS_ChannelGet3DPosition
* EAX functions
	BASS_SetEAXParameters
	BASS_GetEAXParameters
	BASS_ChannelSetEAXMix
	BASS_ChannelGetEAXMix
* Other new functions
	BASS_GetDeviceDescription
	BASS_SetBufferLen
	BASS_ChannelGetFlags
	BASS_ChannelPause
	BASS_ChannelResume
	BASS_ChannelSetPosition
* Replaced function
	BASS_CDResume -> BASS_ChannelResume
* Alterations made to
	BASS_Init
	BASS_CDInit
	BASS_SampleLoad
	BASS_StreamPlay
	BASS_INFO structure
	BASS_SAMPLE structure
	BASS_DEVICE_xxx flags
	BASS_SAMPLE_xxx flags

0.3 - 8/3/1999
--------------
* Synchronization functions
	BASS_ChannelSetSync
	BASS_ChannelClearSync
* Other new functions
	BASS_GetVersion
	BASS_ChannelGetPosition
	BASS_ChannelGetLevel
	BASS_ChannelGetAttributes
	BASS_ChannelSetAttributes
* Replaced functions
	BASS_MusicStop -> BASS_ChannelStop
	BASS_MusicSetVolume -> BASS_ChannelSetAttributes
	BASS_CDStop -> BASS_ChannelStop
	BASS_CDSetVolume -> BASS_ChannelSetAttributes
	BASS_CDGetVolume -> BASS_ChannelGetAttributes
	BASS_ChannelUpdate -> BASS_ChannelSetAttributes
* Alterations made to
	BASS_MusicPlayEx
	BASS_StreamPlay
	BASS_INFO structure

0.2 - 28/2/1999
---------------
* First public release


Bug reports, Suggestions, Comments, Enquiries, etc
==================================================
If you have any of the aforementioned please visit the BASS forum at the
website. If you can't find an answer there, you can also email:

	bass@un4seen.com

