if (!($Host.Name -eq "Package Manager Host")){
    echo "This script will only work when run from Package Manager Console"
    echo "That is needed to correctly install NuGet packages"
    exit 1
}

$CFSversion = "v0.2.5"
if (!(Test-Path ".\CodeFileSanity.$CFSversion.exe") -or !(Get-Command ".\CodeFileSanity.*.exe" -ErrorAction SilentlyContinue)){
    echo "Downloading CodeFileSanity"
    Remove-Item * -Include CodeFileSanity.*.exe
    wget "https://github.com/peppy/CodeFileSanity/releases/download/$CFSversion/CodeFileSanity.exe" -outfile "CodeFileSanity.$CFSversion.exe"
}

echo "Running CodeFileSanity"
.\CodeFileSanity.*.exe
if (!$?){
    exit $LastExitCode
}
echo "CodeFileSanity OK"

Install-Package JetBrains.ReSharper.CommandLineTools
Install-Package NVika.MSBuild
InspectCode .\osu.sln --o="inspectcodereport.xml"
NVika parsereport "inspectcodereport.xml" --treatwarningsaserrors
exit $LastExitCode
