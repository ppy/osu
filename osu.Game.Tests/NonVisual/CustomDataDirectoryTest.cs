// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class CustomDataDirectoryTest
    {
        [Test]
        public void TestDefaultDirectory()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestDefaultDirectory)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var storage = osu.Dependencies.Get<Storage>();

                    string defaultStorageLocation = Path.Combine(Environment.CurrentDirectory, "headless", nameof(TestDefaultDirectory));

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(defaultStorageLocation));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private string customPath => Path.Combine(Environment.CurrentDirectory, "custom-path");

        [Test]
        public void TestCustomDirectory()
        {
            using (var host = new HeadlessGameHost(nameof(TestCustomDirectory)))
            {
                string headlessPrefix = Path.Combine("headless", nameof(TestCustomDirectory));

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(headlessPrefix, host);
                // manual cleaning so we can prepare a config file.
                storage.DeleteDirectory(string.Empty);

                using (var storageConfig = new StorageConfigManager(storage))
                    storageConfig.Set(StorageConfig.FullPath, customPath);

                try
                {
                    var osu = loadOsu(host);

                    // switch to DI'd storage
                    storage = osu.Dependencies.Get<Storage>();

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(customPath));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestSubDirectoryLookup()
        {
            using (var host = new HeadlessGameHost(nameof(TestSubDirectoryLookup)))
            {
                string headlessPrefix = Path.Combine("headless", nameof(TestSubDirectoryLookup));

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(headlessPrefix, host);
                // manual cleaning so we can prepare a config file.
                storage.DeleteDirectory(string.Empty);

                using (var storageConfig = new StorageConfigManager(storage))
                    storageConfig.Set(StorageConfig.FullPath, customPath);

                try
                {
                    var osu = loadOsu(host);

                    // switch to DI'd storage
                    storage = osu.Dependencies.Get<Storage>();

                    string actualTestFile = Path.Combine(customPath, "rulesets", "test");

                    File.WriteAllText(actualTestFile, "test");

                    var rulesetStorage = storage.GetStorageForDirectory("rulesets");
                    var lookupPath = rulesetStorage.GetFiles(".").Single();

                    Assert.That(lookupPath, Is.EqualTo("test"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private OsuGameBase loadOsu(GameHost host)
        {
            var osu = new OsuGameBase();
            Task.Run(() => host.Run(osu));
            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");
            return osu;
        }

        private static void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
    }
}
