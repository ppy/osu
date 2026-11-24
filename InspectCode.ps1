$ErrorActionPreference = "Stop"

dotnet tool restore
# clean is required to ensure all code style errors are (re-)raised by compiler
dotnet clean ./osu.Desktop.slnf --verbosity=q
dotnet build -c Debug -warnaserror osu.Desktop.slnf -p:EnforceCodeStyleInBuild=true
dotnet CodeFileSanity
dotnet jb inspectcode "osu.Desktop.slnf" --no-build --format=Text --stdout --caches-home="inspectcode" --verbosity=WARN

exit $LASTEXITCODE
