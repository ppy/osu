#addin nuget:?package=Cake.Figlet&version=1.3.0

// folders
var rootDirectory = new DirectoryPath("..");
var outputDirectory = rootDirectory.Combine("out");
var tempDirectory = rootDirectory.Combine("tmp");
var artifactsDirectory = rootDirectory.Combine("artifacts");

Task("Clean")
    .Does(() =>
    {
        Information($"Cleaning {outputDirectory}");
        CleanDirectory(outputDirectory);
        Information($"Cleaning {tempDirectory}");
        CleanDirectory(tempDirectory);
        Information($"Cleaning {artifactsDirectory}");
        CleanDirectory(artifactsDirectory);
    });

Task("Display Header")
    .Does(() =>
    {
        Information(Figlet("osu-deploy"));
        Warning("Please note that OSU! and PPY are registered trademarks and as such covered by trademark law.");
        Warning("Do not distribute builds of this project publicly that make use of these.");
    });