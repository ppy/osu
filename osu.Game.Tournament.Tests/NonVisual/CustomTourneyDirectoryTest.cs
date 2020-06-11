// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tournament.Configuration;
using osu.Game.Tournament.IO;
using osu.Game.Tests;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class CustomTourneyDirectoryTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TestDefaultDirectory()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestDefaultDirectory)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var storage = osu.Dependencies.Get<TournamentStorage>();
                    var defaultStorage = Path.Combine(tournamentBasePath(nameof(TestDefaultDirectory)), "default");
                    Assert.That(storage.GetFullPath("."), Is.EqualTo(defaultStorage));
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
            using (HeadlessGameHost host = new HeadlessGameHost(nameof(TestCustomDirectory)))
            {
                string osuDesktopStorage = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestCustomDirectory));
                const string custom_tournament = "custom";

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(osuDesktopStorage, host);
                // manual cleaning so we can prepare a config file.
                storage.DeleteDirectory(string.Empty);

                using (var storageConfig = new TournamentStorageManager(storage))
                    storageConfig.Set(StorageConfig.CurrentTournament, custom_tournament);

                try
                {
                    var osu = loadOsu(host);

                    storage = osu.Dependencies.Get<TournamentStorage>();

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(Path.Combine(tournamentBasePath(nameof(TestCustomDirectory)), "custom")));
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
            using (HeadlessGameHost host = new HeadlessGameHost(nameof(TestMigration)))
            {
                // Recreate the old setup that uses "tournament" as the base path.
                string basePath = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestMigration), "tournament");

                string videosPath = Path.Combine(basePath, "videos");
                string modsPath = Path.Combine(basePath, "mods");
                string flagsPath = Path.Combine(basePath, "flags");

                Directory.CreateDirectory(videosPath);
                Directory.CreateDirectory(modsPath);
                Directory.CreateDirectory(flagsPath);

                string bracketFile = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestMigration), "bracket.json");
                string videoFile = Path.Combine(videosPath, "video.mp4");
                string modFile = Path.Combine(modsPath, "mod.png");
                string flagFile = Path.Combine(flagsPath, "flag.png");

                File.WriteAllText(bracketFile, "{}");

                File.WriteAllText(videoFile, "test");
                File.WriteAllText(modFile, "test");
                File.WriteAllText(flagFile, "test");

                try
                {
                    var osu = loadOsu(host);

                    var storage = osu.Dependencies.Get<TournamentStorage>();

                    var migratedPath = Path.Combine(tournamentBasePath(nameof(TestMigration)), "default");

                    videosPath = Path.Combine(migratedPath, "videos");
                    modsPath = Path.Combine(migratedPath, "mods");
                    flagsPath = Path.Combine(migratedPath, "flags");

                    videoFile = Path.Combine(videosPath, "video.mp4");
                    modFile = Path.Combine(modsPath, "mod.png");
                    flagFile = Path.Combine(flagsPath, "flag.png");

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(migratedPath));
                    Assert.That(storage.GetFiles(".", "bracket.json").Single(), Is.EqualTo("bracket.json"));
                    Assert.True(storage.Exists(videoFile));
                    Assert.True(storage.Exists(modFile));
                    Assert.True(storage.Exists(flagFile));
                }
                finally
                {
                    // Cleaning up after ourselves.
                    host.Storage.Delete("tournament.ini");
                    host.Storage.DeleteDirectory("tournaments");

                    host.Exit();
                }
            }
        }

        private TournamentGameBase loadOsu(GameHost host)
        {
            var osu = new TournamentGameBase();
            Task.Run(() => host.Run(osu));
            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");
            return osu;
        }

        private static void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 90000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
        private string tournamentBasePath(string testInstance) => Path.Combine(RuntimeInfo.StartupDirectory, "headless", testInstance, "tournaments");
    }
}