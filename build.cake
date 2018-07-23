#tool Microsoft.TestPlatform.Portable

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Test");
var framework = Argument("framework", "net471");
var configuration = Argument("configuration", "Debug");

var osuDesktop = new FilePath("./osu.Desktop/osu.Desktop.csproj");

var testProjects = new [] {
    new FilePath("./osu.Game.Tests/osu.Game.Tests.csproj"),
    new FilePath("./osu.Game.Rulesets.Osu.Tests/osu.Game.Rulesets.Osu.Tests.csproj"),
    new FilePath("./osu.Game.Rulesets.Catch.Tests/osu.Game.Rulesets.Catch.Tests.csproj"),
    new FilePath("./osu.Game.Rulesets.Mania.Tests/osu.Game.Rulesets.Mania.Tests.csproj"),
    new FilePath("./osu.Game.Rulesets.Taiko.Tests/osu.Game.Rulesets.Taiko.Tests.csproj"),
};

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Compile")
.Does(() => {
    DotNetCoreBuild(osuDesktop.FullPath, new DotNetCoreBuildSettings {
        Framework = framework,
        Configuration = "Debug"
    });
});

Task("CompileTests")
.DoesForEach(testProjects, testProject => {
    DotNetCoreBuild(testProject.FullPath, new DotNetCoreBuildSettings {
        Framework = framework
    });
});


Task("Test")
.IsDependentOn("CompileTests")
.Does(() => {
    VSTest($"./*.Tests/bin/{configuration}/{framework}/**/*Tests.exe");
});

RunTarget(target);