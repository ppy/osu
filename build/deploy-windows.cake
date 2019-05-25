#load "deploy-base.cake"
#addin "nuget:?package=Octokit&version=0.32.0"

using Octokit;

var target = Argument("target", "Deploy");

// github
string githubUserName = "ppy";
string githubRepoName = "osu";
bool uploadToGithub = Argument("uploadToGithub", false);
var gitHubClient = new GitHubClient(new ProductHeaderValue("osu-deploy"));

var previousReleasesDirectory = tempDirectory.Combine("previousReleases");

bool incrementVersion = Argument("incrementVersion", true);


// desktop project
var desktopProject = rootDirectory.CombineWithFilePath("osu.Desktop/osu.Desktop.csproj");
var desktopOutputDirectory = outputDirectory.Combine("osu.Desktop");
string framework = "netcoreapp2.2";
string runtime = "win-x64";
string configuration = "Release";
string version;

Task("Determine Version")
    .Does(() => 
    {
        var latestRelease = gitHubClient.Repository.Release.GetLatest(githubUserName, githubRepoName).GetAwaiter().GetResult();

        if (latestRelease == null)
            Information("This is the first GitHub release");
        else 
        {
            Information($"Last GitHub release was {latestRelease.Name}");

            if (latestRelease.Draft)
                Warning("WARNING: This is a pending draft release! You might not want to push a build with this present");
        }

        string verBase = DateTime.Now.ToString("yyyy.Mdd.");
        int increment = 0;

        // increase the patch if this is not the first release today
        if (latestRelease?.TagName.StartsWith(verBase) ?? false)
            increment = int.Parse(latestRelease.TagName.Split('.')[2]) + (incrementVersion ? 1 : 0);

        version = $"{verBase}{increment}";
        Information($"Determined version: {version}");
    });

Task("Update Appveyor Version")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .Does(() => 
    {
        AppVeyor.UpdateBuildVersion(version);
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
    .IsDependentOn("Determine Version")
    .IsDependentOn("Update Appveyor Version")
    .IsDependentOn("Clean")
    .IsDependentOn("Dotnet Publish");

RunTarget(target);