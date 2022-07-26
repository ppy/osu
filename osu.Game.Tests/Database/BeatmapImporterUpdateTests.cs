// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using Realms;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace osu.Game.Tests.Database
{
    /// <summary>
    /// Tests the flow where a beatmap is already loaded and an update is applied.
    /// </summary>
    [TestFixture]
    public class BeatmapImporterUpdateTests : RealmTest
    {
        [Test]
        public void TestNewDifficultyAdded()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var rulesets = new RealmRulesetStore(realm, storage);

                using var __ = getBeatmapArchive(out string pathOriginal);
                using var _ = getBeatmapArchiveWithModifications(out string pathMissingOneBeatmap, directory =>
                {
                    // remove one difficulty before first import
                    directory.GetFiles("*.osu").First().Delete();
                });

                var importBeforeUpdate = await importer.Import(new ImportTask(pathMissingOneBeatmap));

                Assert.That(importBeforeUpdate, Is.Not.Null);
                Debug.Assert(importBeforeUpdate != null);

                checkCount<BeatmapSetInfo>(realm, 1, s => !s.DeletePending);
                Assert.That(importBeforeUpdate.Value.Beatmaps, Has.Count.EqualTo(11));

                // Second import matches first but contains one extra .osu file.
                var importAfterUpdate = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOriginal), importBeforeUpdate.Value);

                Assert.That(importAfterUpdate, Is.Not.Null);
                Debug.Assert(importAfterUpdate != null);

                checkCount<BeatmapInfo>(realm, 12);
                checkCount<BeatmapMetadata>(realm, 12);
                checkCount<BeatmapSetInfo>(realm, 1);

                // check the newly "imported" beatmap is not the original.
                Assert.That(importBeforeUpdate.ID, Is.Not.EqualTo(importAfterUpdate.ID));
            });
        }

        private static void checkCount<T>(RealmAccess realm, int expected, Expression<Func<T, bool>>? condition = null) where T : RealmObject
        {
            var query = realm.Realm.All<T>();

            if (condition != null)
                query = query.Where(condition);

            Assert.That(query, Has.Count.EqualTo(expected));
        }

        private static IDisposable getBeatmapArchiveWithModifications(out string path, Action<DirectoryInfo> applyModifications)
        {
            var cleanup = getBeatmapArchive(out path);

            string extractedFolder = $"{path}_extracted";
            Directory.CreateDirectory(extractedFolder);

            using (var zip = ZipArchive.Open(path))
                zip.WriteToDirectory(extractedFolder);

            applyModifications(new DirectoryInfo(extractedFolder));

            File.Delete(path);

            using (var zip = ZipArchive.Create())
            {
                zip.AddAllFromDirectory(extractedFolder);
                zip.SaveTo(path, new ZipWriterOptions(CompressionType.Deflate));
            }

            Directory.Delete(extractedFolder, true);

            return cleanup;
        }

        private static IDisposable getBeatmapArchive(out string path, bool quick = true)
        {
            string beatmapPath = TestResources.GetTestBeatmapForImport(quick);

            path = beatmapPath;

            return new InvokeOnDisposal(() => File.Delete(beatmapPath));
        }
    }
}
