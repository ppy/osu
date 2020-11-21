#addin "nuget:?package=CodeFileSanity&version=0.0.36"

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

Task("InspectCode")
    .Does(() => {
        var inspectcodereport = "inspectcodereport.xml";
        var cacheDir = "inspectcode";
        var verbosity = AppVeyor.IsRunningOnAppVeyor ? "WARN" : "INFO"; // Don't flood CI output

        DotNetCoreTool(rootDirectory.FullPath,
            "jb", $@"inspectcode ""{desktopSlnf}"" --output=""{inspectcodereport}"" --caches-home=""{cacheDir}"" --verbosity={verbosity}");
        DotNetCoreTool(rootDirectory.FullPath, "nvika", $@"parsereport ""{inspectcodereport}"" --treatwarningsaserrors");
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