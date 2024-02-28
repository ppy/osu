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
            using (HeadlessGameHost host = new TestRunHeadlessGameHost(nameof(TestCustomDirectory))) // don't use clean run as we are writing a config file.
            {
                string osuDesktopStorage = Path.Combine(host.UserStoragePaths.First(), nameof(TestCustomDirectory));
                const string custom_tournament = "custom";

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(osuDesktopStorage, host);
                // manual cleaning so we can prepare a config file.
                storage.DeleteDirectory(string.Empty);

                using (var storageConfig = new TournamentConfigManager(storage))
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
    }
}
