// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class CustomDataDirectoryTest : ImportTest
    {
        [Test]
        public void TestDefaultDirectory()
        {
            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    string defaultStorageLocation = getDefaultLocationFor(nameof(TestDefaultDirectory));

                    var osu = LoadOsuIntoHost(host);
                    var storage = osu.Dependencies.Get<Storage>();

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(defaultStorageLocation));
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
            string customPath = prepareCustomPath();

            using (var host = new CustomTestHeadlessGameHost())
            {
                using (var storageConfig = new StorageConfigManager(host.InitialStorage))
                    storageConfig.SetValue(StorageConfig.FullPath, customPath);

                try
                {
                    var osu = LoadOsuIntoHost(host);

                    // switch to DI'd storage
                    var storage = osu.Dependencies.Get<Storage>();

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
            string customPath = prepareCustomPath();

            using (var host = new CustomTestHeadlessGameHost())
            {
                using (var storageConfig = new StorageConfigManager(host.InitialStorage))
                    storageConfig.SetValue(StorageConfig.FullPath, customPath);

                try
                {
                    var osu = LoadOsuIntoHost(host);

                    // switch to DI'd storage
                    var storage = osu.Dependencies.Get<Storage>();

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
            string customPath = prepareCustomPath();

            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    string defaultStorageLocation = getDefaultLocationFor(nameof(TestMigration));

                    var osu = LoadOsuIntoHost(host);
                    var storage = osu.Dependencies.Get<Storage>();
                    var osuStorage = storage as MigratableStorage;

                    // Store the current storage's path. We'll need to refer to this for assertions in the original directory after the migration completes.
                    string originalDirectory = storage.GetFullPath(".");

                    // ensure we perform a save
                    host.Dependencies.Get<FrameworkConfigManager>().Save();

                    // ensure we "use" cache
                    host.Storage.GetStorageForDirectory("cache");

                    // for testing nested files are not ignored (only top level)
                    host.Storage.GetStorageForDirectory("test-nested").GetStorageForDirectory("cache");

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(defaultStorageLocation));

                    osu.Migrate(customPath);

                    Assert.That(storage.GetFullPath("."), Is.EqualTo(customPath));

                    // ensure cache was not moved
                    Assert.That(Directory.Exists(Path.Combine(originalDirectory, "cache")));

                    // ensure nested cache was moved
                    Assert.That(!Directory.Exists(Path.Combine(originalDirectory, "test-nested", "cache")));
                    Assert.That(storage.ExistsDirectory(Path.Combine("test-nested", "cache")));

                    Assert.That(osuStorage, Is.Not.Null);

                    foreach (var file in osuStorage.IgnoreFiles)
                    {
                        Assert.That(File.Exists(Path.Combine(originalDirectory, file)));
                        Assert.That(storage.Exists(file), Is.False);
                    }

                    foreach (var dir in osuStorage.IgnoreDirectories)
                    {
                        Assert.That(Directory.Exists(Path.Combine(originalDirectory, dir)));
                        Assert.That(storage.ExistsDirectory(dir), Is.False);
                    }

                    Assert.That(new StreamReader(Path.Combine(originalDirectory, "storage.ini")).ReadToEnd().Contains($"FullPath = {customPath}"));
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
            string customPath = prepareCustomPath();
            string customPath2 = prepareCustomPath("-2");

            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

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
            string customPath = prepareCustomPath();

            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

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
            string customPath = prepareCustomPath();

            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

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
            string customPath = prepareCustomPath();

            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

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

        private static string getDefaultLocationFor(string testTypeName)
        {
            string path = Path.Combine(RuntimeInfo.StartupDirectory, "headless", testTypeName);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            return path;
        }

        private string prepareCustomPath(string suffix = "")
        {
            string path = Path.Combine(RuntimeInfo.StartupDirectory, $"custom-path{suffix}");

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            return path;
        }

        public class CustomTestHeadlessGameHost : CleanRunHeadlessGameHost
        {
            public Storage InitialStorage { get; }

            public CustomTestHeadlessGameHost([CallerMemberName] string callingMethodName = @"")
                : base(callingMethodName: callingMethodName)
            {
                string defaultStorageLocation = getDefaultLocationFor(callingMethodName);

                InitialStorage = new DesktopStorage(defaultStorageLocation, this);
                InitialStorage.DeleteDirectory(string.Empty);
            }
        }
    }
}
