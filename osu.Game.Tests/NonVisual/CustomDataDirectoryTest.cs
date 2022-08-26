// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
            using (prepareCustomPath(out string customPath))
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
            using (prepareCustomPath(out string customPath))
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
                }
            }
        }

        [Test]
        public void TestMigration()
        {
            using (prepareCustomPath(out string customPath))
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

                    // In the following tests, realm files are ignored as
                    // - in the case of checking the source, interacting with the pipe files (.realm.note) may
                    //   lead to unexpected behaviour.
                    // - in the case of checking the destination, the files may have already been recreated by the game
                    //   as part of the standard migration flow.

                    foreach (string file in osuStorage.IgnoreFiles)
                    {
                        if (!file.Contains(".realm", StringComparison.Ordinal))
                        {
                            Assert.That(File.Exists(Path.Combine(originalDirectory, file)));
                            Assert.That(storage.Exists(file), Is.False, () => $"{file} exists in destination when it was expected to be ignored");
                        }
                    }

                    foreach (string dir in osuStorage.IgnoreDirectories)
                    {
                        if (!dir.Contains(".realm", StringComparison.Ordinal))
                        {
                            Assert.That(Directory.Exists(Path.Combine(originalDirectory, dir)));
                            Assert.That(storage.Exists(dir), Is.False, () => $"{dir} exists in destination when it was expected to be ignored");
                        }
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
            using (prepareCustomPath(out string customPath))
            using (prepareCustomPath(out string customPath2))
            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.That(File.Exists(Path.Combine(customPath, OsuGameBase.CLIENT_DATABASE_FILENAME)));

                    Assert.DoesNotThrow(() => osu.Migrate(customPath2));
                    Assert.That(File.Exists(Path.Combine(customPath2, OsuGameBase.CLIENT_DATABASE_FILENAME)));

                    // some files may have been left behind for whatever reason, but that's not what we're testing here.
                    cleanupPath(customPath);

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.That(File.Exists(Path.Combine(customPath, OsuGameBase.CLIENT_DATABASE_FILENAME)));
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
            using (prepareCustomPath(out string customPath))
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
        public void TestMigrationFailsOnExistingData()
        {
            using (prepareCustomPath(out string customPath))
            using (prepareCustomPath(out string customPath2))
            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    var storage = osu.Dependencies.Get<Storage>();
                    var osuStorage = storage as OsuStorage;

                    string originalDirectory = storage.GetFullPath(".");

                    Assert.DoesNotThrow(() => osu.Migrate(customPath));
                    Assert.That(File.Exists(Path.Combine(customPath, OsuGameBase.CLIENT_DATABASE_FILENAME)));

                    Directory.CreateDirectory(customPath2);
                    File.WriteAllText(Path.Combine(customPath2, OsuGameBase.CLIENT_DATABASE_FILENAME), "I am a text");

                    // Fails because file already exists.
                    Assert.Throws<ArgumentException>(() => osu.Migrate(customPath2));

                    osuStorage?.ChangeDataPath(customPath2);

                    Assert.That(osuStorage?.CustomStoragePath, Is.EqualTo(customPath2));
                    Assert.That(new StreamReader(Path.Combine(originalDirectory, "storage.ini")).ReadToEnd().Contains($"FullPath = {customPath2}"));
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
            using (prepareCustomPath(out string customPath))
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
            using (prepareCustomPath(out string customPath))
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

        [Test]
        public void TestBackupCreatedOnCorruptRealm()
        {
            using (var host = new CustomTestHeadlessGameHost())
            {
                try
                {
                    File.WriteAllText(host.InitialStorage.GetFullPath(OsuGameBase.CLIENT_DATABASE_FILENAME, true), "i am definitely not a realm file");

                    LoadOsuIntoHost(host);

                    Assert.That(host.InitialStorage.GetFiles(string.Empty, "*_corrupt.realm"), Has.One.Items);
                }
                finally
                {
                    host.Exit();
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

        private static IDisposable prepareCustomPath(out string path)
        {
            path = Path.Combine(TestRunHeadlessGameHost.TemporaryTestDirectory, $"custom-path-{Guid.NewGuid()}");
            return new InvokeOnDisposal<string>(path, cleanupPath);
        }

        private static void cleanupPath(string path)
        {
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch
            {
            }
        }

        public class CustomTestHeadlessGameHost : CleanRunHeadlessGameHost
        {
            public Storage InitialStorage { get; }

            public CustomTestHeadlessGameHost([CallerMemberName] string callingMethodName = @"")
                : base(callingMethodName: callingMethodName, bypassCleanupOnSetup: true)
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
