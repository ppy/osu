// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Framework.Testing;
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
                    string defaultStorageLocation = getDefaultLocationFor(host);

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
                    cleanupPath(customPath);
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
                    string lookupPath = rulesetStorage.GetFiles(".").Single();

                    Assert.That(lookupPath, Is.EqualTo("test"));
                }
                finally
                {
                    host.Exit();
                    cleanupPath(customPath);
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
                    string defaultStorageLocation = getDefaultLocationFor(host);

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

                    foreach (string file in osuStorage.IgnoreFiles)
                    {
                        // avoid touching realm files which may be a pipe and break everything.
                        // this is also done locally inside OsuStorage via the IgnoreFiles list.
                        if (file.EndsWith(".ini", StringComparison.Ordinal))
                            Assert.That(File.Exists(Path.Combine(originalDirectory, file)));
                        Assert.That(storage.Exists(file), Is.False);
                    }

                    foreach (string dir in osuStorage.IgnoreDirectories)
                    {
                        Assert.That(Directory.Exists(Path.Combine(originalDirectory, dir)));
                        Assert.That(storage.ExistsDirectory(dir), Is.False);
                    }

                    Assert.That(new StreamReader(Path.Combine(originalDirectory, "storage.ini")).ReadToEnd().Contains($"FullPath = {customPath}"));
                }
                finally
                {
                    host.Exit();
                    cleanupPath(customPath);
                }
            }
        }

        [Test]
        public void TestMigrationBetweenTwoTargets()
        {
            string customPath = prepareCustomPath();
            string customPath2 = prepareCustomPath();

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

                    // some files may have been left behind for whatever reason, but that's not what we're testing here.
                    cleanupPath(customPath);

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.That(File.Exists(Path.Combine(customPath, database_filename)));
                }
                finally
                {
                    host.Exit();
                    cleanupPath(customPath);
                    cleanupPath(customPath2);
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
                    cleanupPath(customPath);
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
                    cleanupPath(customPath);
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
                    cleanupPath(customPath);
                }
            }
        }

        private static string getDefaultLocationFor(CustomTestHeadlessGameHost host)
        {
            string path = Path.Combine(TestRunHeadlessGameHost.TemporaryTestDirectory, host.Name);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            return path;
        }

        private static string prepareCustomPath() => Path.Combine(TestRunHeadlessGameHost.TemporaryTestDirectory, $"custom-path-{Guid.NewGuid()}");

        private static void cleanupPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
            }
        }

        public class CustomTestHeadlessGameHost : CleanRunHeadlessGameHost
        {
            public Storage InitialStorage { get; }

            public CustomTestHeadlessGameHost([CallerMemberName] string callingMethodName = @"")
                : base(callingMethodName: callingMethodName)
            {
                string defaultStorageLocation = getDefaultLocationFor(this);

                InitialStorage = new DesktopStorage(defaultStorageLocation, this);
                InitialStorage.DeleteDirectory(string.Empty);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                try
                {
                    // the storage may have changed from the initial location.
                    // this handles cleanup of the initial location.
                    InitialStorage.DeleteDirectory(string.Empty);
                }
                catch { }
            }
        }
    }
}
