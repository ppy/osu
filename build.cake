///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var framework = Argument("framework", "net471");
var configuration = Argument("configuration", "Release");

var osuDesktop = new FilePath("./osu.Desktop/osu.Desktop.csproj");

var testProjects = GetFiles("**/*.Tests.csproj");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Compile")
.Does(() => {
    DotNetCoreBuild(osuDesktop.FullPath, new DotNetCoreBuildSettings {
        Framework = framework,
        Configuration = configuration
    });
});

Task("Test")
.ContinueOnError()
.DoesForEach(testProjects, testProject => {
    DotNetCoreTest(testProject.FullPath, new DotNetCoreTestSettings {3
        Framework = framework,
        Configuration = configuration,
        Logger = $"trx;LogFileName={testProject.GetFilename()}.trx",
        ResultsDirectory = "./TestResults/"
    });
});

Task("Build")
.IsDependentOn("Compile")
.IsDependentOn("Test");

RunTarget(target);