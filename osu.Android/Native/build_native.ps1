param (
    [string]$NdkPath = $env:ANDROID_NDK_HOME
)

if (-not $NdkPath) {
    Write-Error "ANDROID_NDK_HOME is not set."
    exit 1
}

$ScriptRoot = $PSScriptRoot
$abis = @("armeabi-v7a", "arm64-v8a", "x86", "x86_64")
$cmakeToolchain = Join-Path $NdkPath "build/cmake/android.toolchain.cmake"

# Ensure Ninja is in PATH
if (-not (Get-Command "ninja" -ErrorAction SilentlyContinue)) {
    Write-Host "Ninja not found in PATH. Checking Chocolatey install locations..."
    $chocoBin = Join-Path $env:ProgramData "chocolatey/bin"
    if ($env:ChocolateyInstall) {
        $chocoBin = Join-Path $env:ChocolateyInstall "bin"
    }

    $ninjaExe = Join-Path $chocoBin "ninja.exe"
    if (Test-Path $ninjaExe) {
        Write-Host "Ninja found at $ninjaExe. Adding to PATH."
        $env:PATH = "$chocoBin;$env:PATH"
    } else {
        Write-Error "Ninja not found. Please ensure Ninja is installed and in your PATH."
        exit 1
    }
}

# Print versions for debugging
cmake --version
ninja --version

Write-Host "Building Android Native Libraries..."
Write-Host "NDK Path: $NdkPath"

foreach ($abi in $abis) {
    $buildDir = Join-Path $ScriptRoot "build/$abi"
    $outputDir = Join-Path $ScriptRoot "lib/$abi"

    Write-Host "Building for $abi..."

    if (-not (Test-Path $buildDir)) {
        New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
    }
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    }

    Push-Location $buildDir

    # Use splatting to safely pass arguments
    $cmakeArgs = @(
        "-G", "Ninja",
        "-DCMAKE_TOOLCHAIN_FILE=$cmakeToolchain",
        "-DANDROID_ABI=$abi",
        "-DANDROID_PLATFORM=android-31",
        "-DCMAKE_BUILD_TYPE=Release",
        $ScriptRoot
    )

    Write-Host "Running CMake with args: $cmakeArgs"
    cmake @cmakeArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "CMake configuration failed for $abi"
        Pop-Location
        exit 1
    }

    cmake --build .

    if ($LASTEXITCODE -ne 0) {
        Write-Error "CMake build failed for $abi"
        Pop-Location
        exit 1
    }

    Pop-Location

    Copy-Item "$buildDir/libosu.Android.Native.so" "$outputDir/" -Force
    Write-Host "Built and copied libosu.Android.Native.so for $abi"
}

Write-Host "Android Native Libraries Build Complete."
