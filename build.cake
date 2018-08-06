#addin "nuget:http://localhost:8081/repository/hm?package=CodeFileSanity&version=1.0.10"
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

Task("InspectCode")
.IsDependentOn("Compile")
.Does(() => {
    var nVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();
    var nugetToolPath = GetFiles("./tools/NuGet.CommandLine.*/tools/NuGet.exe").First();

    if (!IsRunningOnWindows()) {
        RunInpectCodeInMono(nugetToolPath, nVikaToolPath);
        return;
    }

    StartProcess(nugetToolPath, $"restore {osuSolution}");

    InspectCode(osuSolution, new InspectCodeSettings {
        CachesHome = "inspectcode",
        OutputFile = "inspectcodereport.xml",
    });

    StartProcess(nVikaToolPath, @"parsereport ""inspectcodereport.xml"" --treatwarningsaserrors");
});

void RunInpectCodeInMono(FilePath nugetToolPath, FilePath nVikaToolPath) {
    var inspectcodeToolPath = GetFiles("./tools/NuGet.CommandLine.*/tools/NuGet.exe").First();

    if (StartProcess("mono", "--version") != 0) {
        Information("Running on an os other than windows and mono is not installed. Skipping InpectCode.");
        return;
    }

    StartProcess("mono", $"{nugetToolPath} restore {osuSolution}");

    StartProcess("mono", $@"{inspectcodeToolPath} --o=""inspectcodereport.xml"" --caches-home=""inspectcode"" ");

    StartProcess("mono", $@"{nVikaToolPath} parsereport ""inspectcodereport.xml"" --treatwarningsaserrors");
}

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