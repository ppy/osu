#!/bin/bash

dotnet tool restore

# Run CodeFileSanity if .NET 6 is available, otherwise skip with a warning
if dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 6."; then
    echo "Running CodeFileSanity..."
    dotnet CodeFileSanity
else
    echo "Warning: CodeFileSanity skipped - requires .NET 6.0 runtime (only .NET 8.0 available)"
fi

dotnet jb inspectcode "osu.Desktop.slnf" --no-build --output="inspectcodereport.xml" --caches-home="inspectcode" --verbosity=WARN
dotnet nvika parsereport "inspectcodereport.xml" --treatwarningsaserrors
