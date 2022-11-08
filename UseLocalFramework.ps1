# Run this script to use a local copy of osu-framework rather than fetching it from nuget.
# It expects the osu-framework directory to be at the same level as the osu directory
#
# https://github.com/ppy/osu-framework/wiki/Testing-local-framework-checkout-with-other-projects

$CSPROJ="osu.Game/osu.Game.csproj"
$SLN="osu.sln"

dotnet remove $CSPROJ package ppy.osu.Framework;
dotnet sln $SLN add ../osu-framework/osu.Framework/osu.Framework.csproj ../osu-framework/osu.Framework.NativeLibs/osu.Framework.NativeLibs.csproj;
dotnet add $CSPROJ reference ../osu-framework/osu.Framework/osu.Framework.csproj

$SLNF=Get-Content "osu.Desktop.slnf" | ConvertFrom-Json
$TMP=New-TemporaryFile
$SLNF.solution.projects += ("../osu-framework/osu.Framework/osu.Framework.csproj", "../osu-framework/osu.Framework.NativeLibs/osu.Framework.NativeLibs.csproj")
ConvertTo-Json $SLNF | Out-File $TMP -Encoding UTF8
Move-Item -Path $TMP -Destination "osu.Desktop.slnf" -Force
