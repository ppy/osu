#addin "nuget:?package=CodeFileSanity"
#addin "nuget:?package=JetBrains.ReSharper.CommandLineTools"
#tool "nuget:?package=NVika.MSBuild"
#tool "nuget:?package=NuGet.CommandLine"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var framework = Argument("framework", "netcoreapp2.1");
var configuration = Argument("configuration", "Release");

var osuDesktop = new FilePath("./osu.Desktop/osu.Desktop.csproj");
var osuSolution = new FilePath("./osu.sln");
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
    DotNetCoreTest(testProject.FullPath, new DotNetCoreTestSettings {
        Framework = framework,
        Configuration = configuration,
        Logger = $"trx;LogFileName={testProject.GetFilename()}.trx",
        ResultsDirectory = "./TestResults/"
    });
});

// windows only because both inspectcore and nvike depend on net45
// will be ignored on linux
Task("InspectCode")
.WithCriteria(IsRunningOnWindows())
.IsDependentOn("Compile")
.Does(() => {
    var NVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();
    var NugetToolPath = GetFiles("./tools/NuGet.CommandLine.*/tools/NuGet.exe").First();

    StartProcess(NugetToolPath, $"restore {osuSolution}");

    InspectCode(osuSolution, new InspectCodeSettings {
        CachesHome = "inspectcode",
        OutputFile = "inspectcodereport.xml",
    });

    StartProcess(NVikaToolPath, @"parsereport ""inspectcodereport.xml"" --treatwarningsaserrors");
});

Task("CodeFileSanity")
.Does(() => {
    ValidateCodeSanity(new ValidateCodeSanitySettings {
        RootDirectory = ".",
        IsAppveyorBuild = AppVeyor.IsRunningOnAppVeyor
    });
});

Task("Build")
.IsDependentOn("CodeFileSanity")
.IsDependentOn("InspectCode")
.IsDependentOn("Test");

RunTarget(target);