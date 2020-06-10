#addin "nuget:?package=CodeFileSanity&version=0.0.36"
#addin "nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2020.1.3"
#tool "nuget:?package=NVika.MSBuild&version=1.0.1"
var nVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "CodeAnalysis");
var configuration = Argument("configuration", "Release");

var rootDirectory = new DirectoryPath("..");
var sln = rootDirectory.CombineWithFilePath("osu.sln");
var desktopSlnf = rootDirectory.CombineWithFilePath("osu.Desktop.slnf");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

// windows only because both inspectcode and nvika depend on net45
Task("InspectCode")
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {
        InspectCode(desktopSlnf, new InspectCodeSettings {
            CachesHome = "inspectcode",
            OutputFile = "inspectcodereport.xml",
            ArgumentCustomization = arg => {
                if (AppVeyor.IsRunningOnAppVeyor) // Don't flood CI output
                    arg.Append("--verbosity:WARN");
                    return arg;
            },
        });

        int returnCode = StartProcess(nVikaToolPath, $@"parsereport ""inspectcodereport.xml"" --treatwarningsaserrors");
        if (returnCode != 0)
            throw new Exception($"inspectcode failed with return code {returnCode}");
    });

Task("CodeFileSanity")
    .Does(() => {
        ValidateCodeSanity(new ValidateCodeSanitySettings {
            RootDirectory = rootDirectory.FullPath,
            IsAppveyorBuild = AppVeyor.IsRunningOnAppVeyor
        });
    });

Task("CodeAnalysis")
    .IsDependentOn("CodeFileSanity")
    .IsDependentOn("InspectCode");

RunTarget(target);