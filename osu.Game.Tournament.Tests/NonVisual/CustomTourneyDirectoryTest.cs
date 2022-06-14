// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Tests;
using osu.Game.Tournament.Configuration;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class CustomTourneyDirectoryTest : TournamentHostTest
    {
        [Test]
        public void TestDefaultDirectory()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadTournament(host);
                    var storage = osu.Dependencies.Get<Storage>();

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(Path.Combine(host.Storage.GetFullPath("."), "tournaments", "default")));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestCustomDirectory()
        {
            using (HeadlessGameHost host = new TestRunHeadlessGameHost(nameof(TestCustomDirectory), null)) // don't use clean run as we are writing a config file.
            {
                string osuDesktopStorage = Path.Combine(host.UserStoragePaths.First(), nameof(TestCustomDirectory));
                const string custom_tournament = "custom";

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(osuDesktopStorage, host);
                // manual cleaning so we can prepare a config file.
                storage.DeleteDirectory(string.Empty);

                using (var storageConfig = new TournamentStorageManager(storage))
                    storageConfig.SetValue(StorageConfig.CurrentTournament, custom_tournament);

                try
                {
                    var osu = LoadTournament(host);

                    storage = osu.Dependencies.Get<Storage>();

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(Path.Combine(host.Storage.GetFullPath("."), "tournaments", custom_tournament)));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestMigration()
        {
            using (HeadlessGameHost host = new TestRunHeadlessGameHost(nameof(TestMigration), null)) // don't use clean run as we are writing test files for migration.
            {
                string osuRoot = Path.Combine(host.UserStoragePaths.First(), nameof(TestMigration));
                string configFile = Path.Combine(osuRoot, "tournament.ini");

                if (File.Exists(configFile))
                    File.Delete(configFile);

                // Recreate the old setup that uses "tournament" as the base path.
                string oldPath = Path.Combine(osuRoot, "tournament");

                string videosPath = Path.Combine(oldPath, "Videos");
                string modsPath = Path.Combine(oldPath, "Mods");
                string flagsPath = Path.Combine(oldPath, "Flags");

                Directory.CreateDirectory(videosPath);
                Directory.CreateDirectory(modsPath);
                Directory.CreateDirectory(flagsPath);

                // Define testing files corresponding to the specific file migrations that are needed
                string bracketFile = Path.Combine(osuRoot, TournamentGameBase.BRACKET_FILENAME);

                string drawingsConfig = Path.Combine(osuRoot, "drawings.ini");
                string drawingsFile = Path.Combine(osuRoot, "drawings.txt");
                string drawingsResult = Path.Combine(osuRoot, "drawings_results.txt");

                // Define sample files to test recursive copying
                string videoFile = Path.Combine(videosPath, "video.mp4");
                string modFile = Path.Combine(modsPath, "mod.png");
                string flagFile = Path.Combine(flagsPath, "flag.png");

                File.WriteAllText(bracketFile, "{}");
                File.WriteAllText(drawingsConfig, "test");
                File.WriteAllText(drawingsFile, "test");
                File.WriteAllText(drawingsResult, "test");
                File.WriteAllText(videoFile, "test");
                File.WriteAllText(modFile, "test");
                File.WriteAllText(flagFile, "test");

                try
                {
                    var osu = LoadTournament(host);

                    var storage = osu.Dependencies.Get<Storage>();

                    string migratedPath = Path.Combine(host.Storage.GetFullPath("."), "tournaments", "default");

                    videosPath = Path.Combine(migratedPath, "Videos");
                    modsPath = Path.Combine(migratedPath, "Mods");
                    flagsPath = Path.Combine(migratedPath, "Flags");

                    videoFile = Path.Combine(videosPath, "video.mp4");
                    modFile = Path.Combine(modsPath, "mod.png");
                    flagFile = Path.Combine(flagsPath, "flag.png");

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(migratedPath));

                    Assert.True(storage.Exists(TournamentGameBase.BRACKET_FILENAME));
                    Assert.True(storage.Exists("drawings.txt"));
                    Assert.True(storage.Exists("drawings_results.txt"));

                    Assert.True(storage.Exists("drawings.ini"));

                    Assert.True(storage.Exists(videoFile));
                    Assert.True(storage.Exists(modFile));
                    Assert.True(storage.Exists(flagFile));
                }
                finally
                {
                    host.Exit();
                }
            }
        }
    }
}
