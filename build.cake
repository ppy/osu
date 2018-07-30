#tool "nuget:?package=JetBrains.ReSharper.CommandLineTools"
#tool "nuget:?package=NVika.MSBuild"
var NVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();
var CodeFileSanityToolPath = DownloadFile("https://github.com/peppy/CodeFileSanity/releases/download/v0.2.5/CodeFileSanity.exe");

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var framework = Argument("framework", "net471");
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

Task("InspectCode")
.Does(() => {
    InspectCode(osuSolution, new InspectCodeSettings {
        CachesHome = "inspectcode",
        OutputFile = "inspectcodereport.xml",
    });

    StartProcess(NVikaToolPath, @"parsereport ""inspectcodereport.xml"" --treatwarningsaserrors");
});

Task("CodeFileSanity")
.Does(() => {
    var result = StartProcess(CodeFileSanityToolPath);

    if (result != 0)
        throw new Exception("Code sanity failed."); 
});

Task("Build")
.IsDependentOn("CodeFileSanity")
.IsDependentOn("Compile")
.IsDependentOn("InspectCode")
.IsDependentOn("Test");

RunTarget(target);