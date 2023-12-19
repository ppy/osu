// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class LegacyBeatmapImporterTest : RealmTest
    {
        private readonly TestLegacyBeatmapImporter importer = new TestLegacyBeatmapImporter();

        [Test]
        public void TestSongsSubdirectories()
        {
            using (var storage = new TemporaryNativeStorage("stable-songs-folder"))
            {
                var songsStorage = storage.GetStorageForDirectory(StableStorage.STABLE_DEFAULT_SONGS_PATH);

                // normal beatmap folder
                var beatmap1 = songsStorage.GetStorageForDirectory("beatmap1");
                createFile(beatmap1, "beatmap.osu");

                // songs subdirectory
                var subdirectory = songsStorage.GetStorageForDirectory("subdirectory");
                createFile(subdirectory, Path.Combine("beatmap2", "beatmap.osu"));
                createFile(subdirectory, Path.Combine("beatmap3", "beatmap.osu"));
                createFile(subdirectory, Path.Combine("sub-subdirectory", "beatmap4", "beatmap.osu"));

                // songs subdirectory with system file
                var subdirectory2 = songsStorage.GetStorageForDirectory("subdirectory2");
                createFile(subdirectory2, ".DS_Store");
                createFile(subdirectory2, Path.Combine("beatmap5", "beatmap.osu"));
                createFile(subdirectory2, Path.Combine("beatmap6", "beatmap.osu"));

                // songs subdirectory with random file
                var subdirectory3 = songsStorage.GetStorageForDirectory("subdirectory3");
                createFile(subdirectory3, "silly readme.txt");
                createFile(subdirectory3, Path.Combine("beatmap7", "beatmap.osu"));

                // empty songs subdirectory
                songsStorage.GetStorageForDirectory("subdirectory3");

                string[] paths = importer.GetStableImportPaths(songsStorage).ToArray();
                Assert.That(paths.Length, Is.EqualTo(7));
                Assert.That(paths.Contains(songsStorage.GetFullPath("beatmap1")));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory", "beatmap2"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory", "beatmap3"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory", "sub-subdirectory", "beatmap4"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory2", "beatmap5"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory2", "beatmap6"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory3", "beatmap7"))));
            }

            static void createFile(Storage storage, string path)
            {
                using (var stream = storage.CreateFileSafely(path))
                    stream.WriteByte(0);
            }
        }

        [Test]
        public void TestStableDateAddedApplied()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
                using (var tmpStorage = new TemporaryNativeStorage("stable-songs-folder"))
                {
                    var stableStorage = new StableStorage(tmpStorage.GetFullPath(""), host);
                    var songsStorage = stableStorage.GetStorageForDirectory(StableStorage.STABLE_DEFAULT_SONGS_PATH);

                    ZipFile.ExtractToDirectory(TestResources.GetQuickTestBeatmapForImport(), songsStorage.GetFullPath("renatus"));

                    string[] beatmaps = Directory.GetFiles(songsStorage.GetFullPath("renatus"), "*.osu", SearchOption.TopDirectoryOnly);

                    File.SetLastWriteTimeUtc(beatmaps[beatmaps.Length / 2], new DateTime(2000, 1, 1, 12, 0, 0));

                    await new LegacyBeatmapImporter(new BeatmapImporter(storage, realm)).ImportFromStableAsync(stableStorage);

                    var importedSet = realm.Realm.All<BeatmapSetInfo>().Single();

                    Assert.NotNull(importedSet);
                    Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)), importedSet.DateAdded);
                }
            });
        }

        private class TestLegacyBeatmapImporter : LegacyBeatmapImporter
        {
            public TestLegacyBeatmapImporter()
                : base(null!)
            {
            }

            public new IEnumerable<string> GetStableImportPaths(Storage storage) => base.GetStableImportPaths(storage);
        }
    }
}
