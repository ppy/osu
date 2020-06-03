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
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.IO;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class CustomDataDirectoryTest
    {
        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(customPath))
                Directory.Delete(customPath, true);
        }

        [Test]
        public void TestDefaultDirectory()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestDefaultDirectory)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var storage = osu.Dependencies.Get<Storage>();

                    string defaultStorageLocation = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestDefaultDirectory));
                    Assert.That(storage.GetFullPath("."), Is.EqualTo(defaultStorageLocation));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private string customPath => Path.Combine(RuntimeInfo.StartupDirectory, "custom-path");

        [Test]
        public void TestCustomDirectory()
        {
            using (var host = new HeadlessGameHost(nameof(TestCustomDirectory)))
            {
                string defaultStorageLocation = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestCustomDirectory));

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(defaultStorageLocation, host);
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
                string defaultStorageLocation = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestSubDirectoryLookup));

                // need access before the game has constructed its own storage yet.
                Storage storage = new DesktopStorage(defaultStorageLocation, host);
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

        [Test]
        public void TestMigration()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestMigration)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var storage = osu.Dependencies.Get<Storage>();

                    // ensure we perform a save
                    host.Dependencies.Get<FrameworkConfigManager>().Save();

                    // ensure we "use" cache
                    host.Storage.GetStorageForDirectory("cache");

                    // for testing nested files are not ignored (only top level)
                    host.Storage.GetStorageForDirectory("test-nested").GetStorageForDirectory("cache");

                    string defaultStorageLocation = Path.Combine(RuntimeInfo.StartupDirectory, "headless", nameof(TestMigration));

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(defaultStorageLocation));

                    osu.Migrate(customPath);

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(customPath));

                    // ensure cache was not moved
                    Assert.That(host.Storage.ExistsDirectory("cache"));

                    // ensure nested cache was moved
                    Assert.That(!host.Storage.ExistsDirectory(Path.Combine("test-nested", "cache")));
                    Assert.That(storage.ExistsDirectory(Path.Combine("test-nested", "cache")));

                    foreach (var file in OsuStorage.IGNORE_FILES)
                    {
                        Assert.That(host.Storage.Exists(file), Is.True);
                        Assert.That(storage.Exists(file), Is.False);
                    }

                    foreach (var dir in OsuStorage.IGNORE_DIRECTORIES)
                    {
                        Assert.That(host.Storage.ExistsDirectory(dir), Is.True);
                        Assert.That(storage.ExistsDirectory(dir), Is.False);
                    }

                    Assert.That(new StreamReader(host.Storage.GetStream("storage.ini")).ReadToEnd().Contains($"FullPath = {customPath}"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestMigrationBetweenTwoTargets()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestMigrationBetweenTwoTargets)))
            {
                try
                {
                    var osu = loadOsu(host);

                    string customPath2 = $"{customPath}-2";

                    const string database_filename = "client.db";

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.That(File.Exists(Path.Combine(customPath, database_filename)));

                    Assert.DoesNotThrow(() => osu.Migrate(customPath2));
                    Assert.That(File.Exists(Path.Combine(customPath2, database_filename)));

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.That(File.Exists(Path.Combine(customPath, database_filename)));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestMigrationToSameTargetFails()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestMigrationToSameTargetFails)))
            {
                try
                {
                    var osu = loadOsu(host);

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.Throws<ArgumentException>(() => osu.Migrate(customPath));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestMigrationToNestedTargetFails()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestMigrationToNestedTargetFails)))
            {
                try
                {
                    var osu = loadOsu(host);

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));

                    string subFolder = Path.Combine(customPath, "sub");

                    if (Directory.Exists(subFolder))
                        Directory.Delete(subFolder, true);

                    Directory.CreateDirectory(subFolder);

                    Assert.Throws<ArgumentException>(() => osu.Migrate(subFolder));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestMigrationToSeeminglyNestedTarget()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestMigrationToSeeminglyNestedTarget)))
            {
                try
                {
                    var osu = loadOsu(host);

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));

                    string seeminglySubFolder = customPath + "sub";

                    if (Directory.Exists(seeminglySubFolder))
                        Directory.Delete(seeminglySubFolder, true);

                    Directory.CreateDirectory(seeminglySubFolder);

                    osu.Migrate(seeminglySubFolder);
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
