// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
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
        public void TestImportThenUpdateWithNewDifficulty()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                var importer = new BeatmapImporter(storage, realm);
                using var store = new RealmRulesetStore(realm, storage);

                string? pathOriginal = TestResources.GetTestBeatmapForImport();

                string pathMissingOneBeatmap = pathOriginal.Replace(".osz", "_missing_difficulty.osz");

                string extractedFolder = $"{pathOriginal}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    using (var zip = ZipArchive.Open(pathOriginal))
                        zip.WriteToDirectory(extractedFolder);

                    // remove one difficulty before first import
                    new FileInfo(Directory.GetFiles(extractedFolder, "*.osu").First()).Delete();

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(pathMissingOneBeatmap, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    var firstImport = await importer.Import(new ImportTask(pathMissingOneBeatmap));
                    Assert.That(firstImport, Is.Not.Null);
                    Debug.Assert(firstImport != null);

                    Assert.That(realm.Realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending), Has.Count.EqualTo(1));
                    Assert.That(realm.Realm.All<BeatmapSetInfo>().First(s => !s.DeletePending).Beatmaps, Has.Count.EqualTo(11));

                    // Second import matches first but contains one extra .osu file.
                    var secondImport = (await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(pathOriginal), firstImport.Value)).FirstOrDefault();
                    Assert.That(secondImport, Is.Not.Null);
                    Debug.Assert(secondImport != null);

                    Assert.That(realm.Realm.All<BeatmapInfo>(), Has.Count.EqualTo(12));
                    Assert.That(realm.Realm.All<BeatmapMetadata>(), Has.Count.EqualTo(12));
                    Assert.That(realm.Realm.All<BeatmapSetInfo>(), Has.Count.EqualTo(1));

                    // check the newly "imported" beatmap is not the original.
                    Assert.That(firstImport.ID, Is.Not.EqualTo(secondImport.ID));
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }
    }
}
