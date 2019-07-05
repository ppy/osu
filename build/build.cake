#addin "nuget:?package=CodeFileSanity&version=0.0.21"
#addin "nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2019.1.1"
#tool "nuget:?package=NVika.MSBuild&version=1.0.1"
var nVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

var rootDirectory = new DirectoryPath("..");
var solution = rootDirectory.CombineWithFilePath("osu.sln");
var androidSolution = rootDirectory.CombineWithFilePath("osu.Android.sln");
var iOSSolution = rootDirectory.CombineWithFilePath("osu.iOS.sln");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Compile")
    .Does(() => {
        DotNetCoreBuild(solution.FullPath, new DotNetCoreBuildSettings {
            Configuration = configuration,
        });
    });

Task("Test")
    .IsDependentOn("Compile")
    .Does(() => {
        var testAssemblies = GetFiles(rootDirectory + "/**/*.Tests/bin/**/*.Tests.dll");

        DotNetCoreVSTest(testAssemblies, new DotNetCoreVSTestSettings {
            Logger = AppVeyor.IsRunningOnAppVeyor ? "Appveyor" : $"trx",
            Parallel = true,
            ToolTimeout = TimeSpan.FromMinutes(10),
        });
    });

// windows only because both inspectcore and nvika depend on net45
Task("InspectCode")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("Compile")
    .DoesForEach(
        new [] { solution, androidSolution, iOSSolution },
        inspectSolution => {
            string reportFileName = $"inspectcodereport.{inspectSolution.GetFilenameWithoutExtension()}.xml";

            InspectCode(inspectSolution, new InspectCodeSettings {
                CachesHome = $"inspectcode{inspectSolution.GetFilenameWithoutExtension()}",
                OutputFile = reportFileName,
            });

            int returnCode = StartProcess(nVikaToolPath, $@"parsereport ""{reportFileName}"" --treatwarningsaserrors");
            if (returnCode != 0)
                throw new Exception($"inspectcode failed with return code {returnCode}");
        }
    );

Task("CodeFileSanity")
    .Does(() => {
        ValidateCodeSanity(new ValidateCodeSanitySettings {
            RootDirectory = rootDirectory.FullPath,
            IsAppveyorBuild = AppVeyor.IsRunningOnAppVeyor
        });
    });

Task("Build")
    .IsDependentOn("CodeFileSanity")
    .IsDependentOn("InspectCode")
    .IsDependentOn("Test");

RunTarget(target);