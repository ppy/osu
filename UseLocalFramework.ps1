# Run this script to use a local copy of osu-framework rather than fetching it from nuget.
# It expects the osu-framework directory to be at the same level as the osu directory
#
# https://github.com/ppy/osu-framework/wiki/Testing-local-framework-checkout-with-other-projects

$GAME_CSPROJ="osu.Game/osu.Game.csproj"
$ANDROID_PROPS="osu.Android.props"
$IOS_PROPS="osu.iOS.props"
$SLN="osu.sln"

dotnet remove $GAME_CSPROJ reference ppy.osu.Framework;
dotnet remove $ANDROID_PROPS reference ppy.osu.Framework.Android;
dotnet remove $IOS_PROPS reference ppy.osu.Framework.iOS;

dotnet sln $SLN add ../osu-framework/osu.Framework/osu.Framework.csproj `
    ../osu-framework/osu.Framework.NativeLibs/osu.Framework.NativeLibs.csproj `
    ../osu-framework/osu.Framework.Android/osu.Framework.Android.csproj `
    ../osu-framework/osu.Framework.iOS/osu.Framework.iOS.csproj;

dotnet add $GAME_CSPROJ reference ../osu-framework/osu.Framework/osu.Framework.csproj;
dotnet add $ANDROID_PROPS reference ../osu-framework/osu.Framework.Android/osu.Framework.Android.csproj;
dotnet add $IOS_PROPS reference ../osu-framework/osu.Framework.iOS/osu.Framework.iOS.csproj;

# workaround for dotnet add not inserting $(MSBuildThisFileDirectory) on props files
(Get-Content "osu.Android.props") -replace "`"..\\osu-framework", "`"`$(MSBuildThisFileDirectory)..\osu-framework" | Set-Content "osu.Android.props"
(Get-Content "osu.iOS.props") -replace "`"..\\osu-framework", "`"`$(MSBuildThisFileDirectory)..\osu-framework" | Set-Content "osu.iOS.props"

# needed because iOS framework nupkg includes a set of properties to work around certain issues during building,
# and those get ignored when referencing framework via project, threfore we have to manually include it via props reference.
(Get-Content "osu.iOS.props") |
    Foreach-Object {
        if ($_ -match "</Project>")
        {
            "  <Import Project=`"`$(MSBuildThisFileDirectory)../osu-framework/osu.Framework.iOS.props`"/>"
        }

        $_
    } | Set-Content "osu.iOS.props"

$TMP=New-TemporaryFile

$SLNF=Get-Content "osu.Desktop.slnf" | ConvertFrom-Json
$SLNF.solution.projects += ("../osu-framework/osu.Framework/osu.Framework.csproj", "../osu-framework/osu.Framework.NativeLibs/osu.Framework.NativeLibs.csproj")
ConvertTo-Json $SLNF | Out-File $TMP -Encoding UTF8
Move-Item -Path $TMP -Destination "osu.Desktop.slnf" -Force

$SLNF=Get-Content "osu.Android.slnf" | ConvertFrom-Json
$SLNF.solution.projects += ("../osu-framework/osu.Framework/osu.Framework.csproj", "../osu-framework/osu.Framework.NativeLibs/osu.Framework.NativeLibs.csproj", "../osu-framework/osu.Framework.Android/osu.Framework.Android.csproj")
ConvertTo-Json $SLNF | Out-File $TMP -Encoding UTF8
Move-Item -Path $TMP -Destination "osu.Android.slnf" -Force

$SLNF=Get-Content "osu.iOS.slnf" | ConvertFrom-Json
$SLNF.solution.projects += ("../osu-framework/osu.Framework/osu.Framework.csproj", "../osu-framework/osu.Framework.NativeLibs/osu.Framework.NativeLibs.csproj", "../osu-framework/osu.Framework.iOS/osu.Framework.iOS.csproj")
ConvertTo-Json $SLNF | Out-File $TMP -Encoding UTF8
Move-Item -Path $TMP -Destination "osu.iOS.slnf" -Force
