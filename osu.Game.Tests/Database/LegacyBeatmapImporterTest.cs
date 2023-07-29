// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class LegacyBeatmapImporterTest
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

                // empty songs subdirectory
                songsStorage.GetStorageForDirectory("subdirectory3");

                string[] paths = importer.GetStableImportPaths(songsStorage).ToArray();
                Assert.That(paths.Length, Is.EqualTo(6));
                Assert.That(paths.Contains(songsStorage.GetFullPath("beatmap1")));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory", "beatmap2"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory", "beatmap3"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory", "sub-subdirectory", "beatmap4"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory2", "beatmap5"))));
                Assert.That(paths.Contains(songsStorage.GetFullPath(Path.Combine("subdirectory2", "beatmap6"))));
            }

            static void createFile(Storage storage, string path)
            {
                using (var stream = storage.CreateFileSafely(path))
                    stream.WriteByte(0);
            }
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
