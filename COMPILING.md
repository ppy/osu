#Linux#
###1. Requirements:###
Mono >= 5.4.0 (>= 5.8.0 recommended)
Please check [here](http://www.mono-project.com/download/) for stable or [here](http://www.mono-project.com/download/alpha/) for an alpha release.
NuGet >= 4.4.0
msbuild
git
###2. Cloning project
Clone the entire repository with submodules using

    git clone https://github.com/ppy/osu --recursive
Then restore NuGet packages from the repository

    nuget restore
We also need OpenTK >= 3.0.0-pre

    nuget install opentk -version=3.0.0-pre
###3. Compiling
Simply run `msbuild` where `osu.sln` is located, this will create all binaries in `osu/osu.Desktop/bin/Debug`.
###4. Optimizing
If you want additional performance you can change build type to Release with
```
msbuild -p:Configuration=Release
```
Additionally, mono provides an AOT utility which attempts to precompile binaries. You can utilize that by running
```
mono --aot ./osu\!.exe
```