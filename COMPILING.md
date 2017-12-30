# Linux
### 1. Requirements:
Mono >= 5.4.0 (>= 5.8.0 recommended)
Please check [here](http://www.mono-project.com/download/) for stable or [here](http://www.mono-project.com/download/alpha/) for an alpha release.  
NuGet >= 4.4.0  
msbuild  
git

### 2. Cloning project
Clone the entire repository with submodules using
```
git clone https://github.com/ppy/osu --recursive
```
Then restore NuGet packages from the repository
```
nuget restore
```
### 3. Compiling
Simply run `msbuild` where `osu.sln` is located, this will create all binaries in `osu/osu.Desktop/bin/Debug`.
### 4. Optimizing
If you want additional performance you can change build type to Release with
```
msbuild -p:Configuration=Release
```
Additionally, mono provides an AOT utility which attempts to precompile binaries. You can utilize that by running
```
mono --aot ./osu\!.exe
```
### 5. Troubleshooting
You may run into trouble with NuGet versioning, as the one in packaging system is almost always out of date. Simply run 
```
nuget
sudo nuget update -self
```
**Warning** NuGet creates few config files when it's run for the first time.
Do not run NuGet as root on the first run or you might run into very peculiar issues.

# macOS
### 1. Requirements:
Mono >= 5.4.0
You can download through homebrew-cask (brew cask install mono-mdk) or [here](http://www.mono-project.com/download/)  
Do **not** download mono via regular homebrew as it does not come with the required libraries and components for development.  
nuget (comes with mono-mdk)  
msbuild (comes with mono-mdk)  
git  

### 2. Cloning project
Clone the entire repository with submodules using
```
git clone --recurse-submodules https://github.com/ppy/osu
```
Navigate into the osu dirctory then restore NuGet packages from the repository
```
nuget restore
```
### 3. Compiling
Run `msbuild` where `osu.sln` is located (should be current directory), binaries will be located in `osu/osu.Desktop/bin/Debug`.
### 4. Optimizing
If you want additional performance you can change build type to Release with
```
msbuild -p:Configuration=Release
```
Additionally, mono provides an AOT utility which attempts to precompile binaries. You can utilize that by running
```
mono --aot ./osu\!.exe
```
### 5. Troubleshooting
If you see several errors referencing `netstandard 2.0.0` it means you have installed `homebrew mono` not `homebrew cask mono-mdk`. Refer back to Requirements.
