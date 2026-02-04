param (
    [string]$NdkPath = $env:ANDROID_NDK_HOME
)

if (-not $NdkPath) {
    Write-Error "ANDROID_NDK_HOME is not set."
    exit 1
}

$abis = @("armeabi-v7a", "arm64-v8a", "x86", "x86_64")
$cmakeToolchain = Join-Path $NdkPath "build/cmake/android.toolchain.cmake"

Write-Host "Building Android Native Libraries..."
Write-Host "NDK Path: $NdkPath"

foreach ($abi in $abis) {
    $buildDir = "build/$abi"
    $outputDir = "lib/$abi"

    Write-Host "Building for $abi..."

    if (-not (Test-Path $buildDir)) {
        New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
    }
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    }

    Push-Location $buildDir

    cmake -G "Ninja" -DCMAKE_TOOLCHAIN_FILE="$cmakeToolchain" -DANDROID_ABI=$abi -DANDROID_PLATFORM=android-21 -DCMAKE_BUILD_TYPE=Release ../..

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
