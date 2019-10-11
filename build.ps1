dotnet tool install Cake.Tool --global --version 0.35.0
dotnet cake ./build/build.cake --bootstrap
dotnet cake ./build/build.cake
exit $LASTEXITCODE