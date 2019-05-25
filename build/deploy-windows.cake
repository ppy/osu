#addin nuget:?package=Cake.Figlet&version=1.3.0

var target = Argument("target", "Deploy");

// folders
var rootDirectory = new DirectoryPath("..");
var outputDirectory = rootDirectory.Combine("out");

// desktop project
var desktopProject = rootDirectory.CombineWithFilePath("osu.Desktop/osu.Desktop.csproj");
var desktopOutputDirectory = outputDirectory.Combine("osu.Desktop");
var framework = "netcoreapp2.2";
var runtime = "win-x64";
var configuration = "Release";

Task("Display Header")
    .Does(() => 
    {
        Information(Figlet("osu-deploy"));
        Warning("Please note that OSU! and PPY are registered trademarks and as such covered by trademark law.");
        Warning("Do not distribute builds of this project publicly that make use of these.");
    });
    
Task("Dotnet Publish")
    .Does(() => 
    {
        DotNetCorePublish(Context.MakeAbsolute(desktopProject).FullPath, new DotNetCorePublishSettings
        {
            Framework = framework,
            Runtime = runtime,
            OutputDirectory = desktopOutputDirectory,
        });
    });

Task("Deploy")
    .IsDependentOn("Display Header")
    .IsDependentOn("Dotnet Publish");

RunTarget(target);