dotnet tool restore

# Temporarily disabled until the tool is upgraded to 5.0.
  # The version specified in .config/dotnet-tools.json (3.1.37601) won't run on .NET hosts >=5.0.7.
  # - cmd: dotnet format --dry-run --check

dotnet CodeFileSanity
dotnet jb inspectcode "osu.Desktop.slnf" --no-build --output="inspectcodereport.xml" --caches-home="inspectcode" --verbosity=WARN
dotnet nvika parsereport "inspectcodereport.xml" --treatwarningsaserrors

exit $LASTEXITCODE
