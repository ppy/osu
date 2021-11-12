// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using osu.Game.Stores;
using osu.Game.Tests.Resources;
using Realms;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

#nullable enable

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class BeatmapImporterTests : RealmTest
    {
        [Test]
        public void TestImportBeatmapThenCleanup()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using (var importer = new BeatmapImporter(realmFactory, storage))
                using (new RealmRulesetStore(realmFactory, storage))
                {
                    ILive<RealmBeatmapSet>? imported;

                    using (var reader = new ZipArchiveReader(TestResources.GetTestBeatmapStream()))
                        imported = await importer.Import(reader);

                    Assert.AreEqual(1, realmFactory.Context.All<RealmBeatmapSet>().Count());

                    Assert.NotNull(imported);
                    Debug.Assert(imported != null);

                    imported.PerformWrite(s => s.DeletePending = true);

                    Assert.AreEqual(1, realmFactory.Context.All<RealmBeatmapSet>().Count(s => s.DeletePending));
                }
            });

            Logger.Log("Running with no work to purge pending deletions");

            RunTestWithRealm((realmFactory, _) => { Assert.AreEqual(0, realmFactory.Context.All<RealmBeatmapSet>().Count()); });
        }

        [Test]
        public void TestImportWhenClosed()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                await LoadOszIntoStore(importer, realmFactory.Context);
            });
        }

        [Test]
        public void TestImportThenDelete()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                deleteBeatmapSet(imported, realmFactory.Context);
            });
        }

        [Test]
        public void TestImportThenDeleteFromStream()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? tempPath = TestResources.GetTestBeatmapForImport();

                ILive<RealmBeatmapSet>? importedSet;

                using (var stream = File.OpenRead(tempPath))
                {
                    importedSet = await importer.Import(new ImportTask(stream, Path.GetFileName(tempPath)));
                    ensureLoaded(realmFactory.Context);
                }

                Assert.NotNull(importedSet);
                Debug.Assert(importedSet != null);

                Assert.IsTrue(File.Exists(tempPath), "Stream source file somehow went missing");
                File.Delete(tempPath);

                var imported = realmFactory.Context.All<RealmBeatmapSet>().First(beatmapSet => beatmapSet.ID == importedSet.ID);

                deleteBeatmapSet(imported, realmFactory.Context);
            });
        }

        [Test]
        public void TestImportThenImport()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Context);
                var importedSecondTime = await LoadOszIntoStore(importer, realmFactory.Context);

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                checkBeatmapSetCount(realmFactory.Context, 1);
                checkSingleReferencedFileCount(realmFactory.Context, 18);
            });
        }

        [Test]
        public void TestImportThenImportWithReZip()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                    string hashBefore = hashFile(temp);

                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(extractedFolder);

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    // zip files differ because different compression or encoder.
                    Assert.AreNotEqual(hashBefore, hashFile(temp));

                    var importedSecondTime = await importer.Import(new ImportTask(temp));

                    ensureLoaded(realmFactory.Context);

                    Assert.NotNull(importedSecondTime);
                    Debug.Assert(importedSecondTime != null);

                    // but contents doesn't, so existing should still be used.
                    Assert.IsTrue(imported.ID == importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.PerformRead(s => s.Beatmaps.First().ID));
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        public void TestImportThenImportWithChangedHashedFile()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                    await createScoreForBeatmap(realmFactory.Context, imported.Beatmaps.First());

                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(extractedFolder);

                    // arbitrary write to hashed file
                    // this triggers the special BeatmapManager.PreImport deletion/replacement flow.
                    using (var sw = new FileInfo(Directory.GetFiles(extractedFolder, "*.osu").First()).AppendText())
                        await sw.WriteLineAsync("// changed");

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    var importedSecondTime = await importer.Import(new ImportTask(temp));

                    ensureLoaded(realmFactory.Context);

                    // check the newly "imported" beatmap is not the original.
                    Assert.NotNull(importedSecondTime);
                    Debug.Assert(importedSecondTime != null);

                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID != importedSecondTime.PerformRead(s => s.Beatmaps.First().ID));
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        [Ignore("intentionally broken by import optimisations")]
        public void TestImportThenImportWithChangedFile()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(extractedFolder);

                    // arbitrary write to non-hashed file
                    using (var sw = new FileInfo(Directory.GetFiles(extractedFolder, "*.mp3").First()).AppendText())
                        await sw.WriteLineAsync("text");

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    var importedSecondTime = await importer.Import(new ImportTask(temp));

                    ensureLoaded(realmFactory.Context);

                    Assert.NotNull(importedSecondTime);
                    Debug.Assert(importedSecondTime != null);

                    // check the newly "imported" beatmap is not the original.
                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID != importedSecondTime.PerformRead(s => s.Beatmaps.First().ID));
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        public void TestImportThenImportWithDifferentFilename()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(extractedFolder);

                    // change filename
                    var firstFile = new FileInfo(Directory.GetFiles(extractedFolder).First());
                    firstFile.MoveTo(Path.Combine(firstFile.DirectoryName.AsNonNull(), $"{firstFile.Name}-changed{firstFile.Extension}"));

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    var importedSecondTime = await importer.Import(new ImportTask(temp));

                    ensureLoaded(realmFactory.Context);

                    Assert.NotNull(importedSecondTime);
                    Debug.Assert(importedSecondTime != null);

                    // check the newly "imported" beatmap is not the original.
                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID != importedSecondTime.PerformRead(s => s.Beatmaps.First().ID));
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        [Ignore("intentionally broken by import optimisations")]
        public void TestImportCorruptThenImport()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                var firstFile = imported.Files.First();

                long originalLength;
                using (var stream = storage.GetStream(firstFile.File.StoragePath))
                    originalLength = stream.Length;

                using (var stream = storage.GetStream(firstFile.File.StoragePath, FileAccess.Write, FileMode.Create))
                    stream.WriteByte(0);

                var importedSecondTime = await LoadOszIntoStore(importer, realmFactory.Context);

                using (var stream = storage.GetStream(firstFile.File.StoragePath))
                    Assert.AreEqual(stream.Length, originalLength, "Corruption was not fixed on second import");

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                checkBeatmapSetCount(realmFactory.Context, 1);
                checkSingleReferencedFileCount(realmFactory.Context, 18);
            });
        }

        [Test]
        public void TestModelCreationFailureDoesntReturn()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var progressNotification = new ImportProgressNotification();

                var zipStream = new MemoryStream();

                using (var zip = ZipArchive.Create())
                    zip.SaveTo(zipStream, new ZipWriterOptions(CompressionType.Deflate));

                var imported = await importer.Import(
                    progressNotification,
                    new ImportTask(zipStream, string.Empty)
                );

                checkBeatmapSetCount(realmFactory.Context, 0);
                checkBeatmapCount(realmFactory.Context, 0);

                Assert.IsEmpty(imported);
                Assert.AreEqual(ProgressNotificationState.Cancelled, progressNotification.State);
            });
        }

        [Test]
        public void TestRollbackOnFailure()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                int loggedExceptionCount = 0;

                Logger.NewEntry += l =>
                {
                    if (l.Target == LoggingTarget.Database && l.Exception != null)
                        Interlocked.Increment(ref loggedExceptionCount);
                };

                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                realmFactory.Context.Write(() => imported.Hash += "-changed");

                checkBeatmapSetCount(realmFactory.Context, 1);
                checkBeatmapCount(realmFactory.Context, 12);
                checkSingleReferencedFileCount(realmFactory.Context, 18);

                string? brokenTempFilename = TestResources.GetTestBeatmapForImport();

                MemoryStream brokenOsu = new MemoryStream();
                MemoryStream brokenOsz = new MemoryStream(await File.ReadAllBytesAsync(brokenTempFilename));

                File.Delete(brokenTempFilename);

                using (var outStream = File.Open(brokenTempFilename, FileMode.CreateNew))
                using (var zip = ZipArchive.Open(brokenOsz))
                {
                    zip.AddEntry("broken.osu", brokenOsu, false);
                    zip.SaveTo(outStream, CompressionType.Deflate);
                }

                // this will trigger purging of the existing beatmap (online set id match) but should rollback due to broken osu.
                try
                {
                    await importer.Import(new ImportTask(brokenTempFilename));
                }
                catch
                {
                }

                checkBeatmapSetCount(realmFactory.Context, 1);
                checkBeatmapCount(realmFactory.Context, 12);

                checkSingleReferencedFileCount(realmFactory.Context, 18);

                Assert.AreEqual(1, loggedExceptionCount);

                File.Delete(brokenTempFilename);
            });
        }

        [Test]
        public void TestImportThenDeleteThenImport()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                deleteBeatmapSet(imported, realmFactory.Context);

                var importedSecondTime = await LoadOszIntoStore(importer, realmFactory.Context);

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);
            });
        }

        [Test]
        public void TestImportThenDeleteThenImportWithOnlineIDsMissing()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Context);

                realmFactory.Context.Write(() =>
                {
                    foreach (var b in imported.Beatmaps)
                        b.OnlineID = -1;
                });

                deleteBeatmapSet(imported, realmFactory.Context);

                var importedSecondTime = await LoadOszIntoStore(importer, realmFactory.Context);

                // check the newly "imported" beatmap has been reimported due to mismatch (even though hashes matched)
                Assert.IsTrue(imported.ID != importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID != importedSecondTime.Beatmaps.First().ID);
            });
        }

        [Test]
        public void TestImportWithDuplicateBeatmapIDs()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                var metadata = new RealmBeatmapMetadata
                {
                    Artist = "SomeArtist",
                    Author =
                    {
                        Username = "SomeAuthor"
                    }
                };

                var ruleset = realmFactory.Context.All<RealmRuleset>().First();

                var toImport = new RealmBeatmapSet
                {
                    OnlineID = 1,
                    Beatmaps =
                    {
                        new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), metadata)
                        {
                            OnlineID = 2,
                        },
                        new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), metadata)
                        {
                            OnlineID = 2,
                            Status = BeatmapSetOnlineStatus.Loved,
                        }
                    }
                };

                var imported = await importer.Import(toImport);

                Assert.NotNull(imported);
                Debug.Assert(imported != null);

                Assert.AreEqual(-1, imported.PerformRead(s => s.Beatmaps[0].OnlineID));
                Assert.AreEqual(-1, imported.PerformRead(s => s.Beatmaps[1].OnlineID));
            });
        }

        [Test]
        public void TestImportWhenFileOpen()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();
                using (File.OpenRead(temp))
                    await importer.Import(temp);
                ensureLoaded(realmFactory.Context);
                File.Delete(temp);
                Assert.IsFalse(File.Exists(temp), "We likely held a read lock on the file when we shouldn't");
            });
        }

        [Test]
        public void TestImportWithDuplicateHashes()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(extractedFolder);

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.AddEntry("duplicate.osu", Directory.GetFiles(extractedFolder, "*.osu").First());
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    await importer.Import(temp);

                    ensureLoaded(realmFactory.Context);
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        public void TestImportNestedStructure()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                string subfolder = Path.Combine(extractedFolder, "subfolder");

                Directory.CreateDirectory(subfolder);

                try
                {
                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(subfolder);

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    var imported = await importer.Import(new ImportTask(temp));

                    Assert.NotNull(imported);
                    Debug.Assert(imported != null);

                    ensureLoaded(realmFactory.Context);

                    Assert.IsFalse(imported.PerformRead(s => s.Files.Any(f => f.Filename.Contains("subfolder"))), "Files contain common subfolder");
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        public void TestImportWithIgnoredDirectoryInArchive()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                string dataFolder = Path.Combine(extractedFolder, "actual_data");
                string resourceForkFolder = Path.Combine(extractedFolder, "__MACOSX");
                string resourceForkFilePath = Path.Combine(resourceForkFolder, ".extracted");

                Directory.CreateDirectory(dataFolder);
                Directory.CreateDirectory(resourceForkFolder);

                using (var resourceForkFile = File.CreateText(resourceForkFilePath))
                {
                    await resourceForkFile.WriteLineAsync("adding content so that it's not empty");
                }

                try
                {
                    using (var zip = ZipArchive.Open(temp))
                        zip.WriteToDirectory(dataFolder);

                    using (var zip = ZipArchive.Create())
                    {
                        zip.AddAllFromDirectory(extractedFolder);
                        zip.SaveTo(temp, new ZipWriterOptions(CompressionType.Deflate));
                    }

                    var imported = await importer.Import(new ImportTask(temp));

                    Assert.NotNull(imported);
                    Debug.Assert(imported != null);

                    ensureLoaded(realmFactory.Context);

                    Assert.IsFalse(imported.PerformRead(s => s.Files.Any(f => f.Filename.Contains("__MACOSX"))), "Files contain resource fork folder, which should be ignored");
                    Assert.IsFalse(imported.PerformRead(s => s.Files.Any(f => f.Filename.Contains("actual_data"))), "Files contain common subfolder");
                }
                finally
                {
                    Directory.Delete(extractedFolder, true);
                }
            });
        }

        [Test]
        public void TestUpdateBeatmapInfo()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapImporter(realmFactory, storage);
                using var store = new RealmRulesetStore(realmFactory, storage);

                string? temp = TestResources.GetTestBeatmapForImport();
                await importer.Import(temp);

                // Update via the beatmap, not the beatmap info, to ensure correct linking
                RealmBeatmapSet setToUpdate = realmFactory.Context.All<RealmBeatmapSet>().First();

                var beatmapToUpdate = setToUpdate.Beatmaps.First();

                realmFactory.Context.Write(() => beatmapToUpdate.DifficultyName = "updated");

                RealmBeatmap updatedInfo = realmFactory.Context.All<RealmBeatmap>().First(b => b.ID == beatmapToUpdate.ID);
                Assert.That(updatedInfo.DifficultyName, Is.EqualTo("updated"));
            });
        }

        public static async Task<RealmBeatmapSet?> LoadQuickOszIntoOsu(BeatmapImporter importer, Realm realm)
        {
            string? temp = TestResources.GetQuickTestBeatmapForImport();

            var importedSet = await importer.Import(new ImportTask(temp));

            Assert.NotNull(importedSet);

            ensureLoaded(realm);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return realm.All<RealmBeatmapSet>().FirstOrDefault(beatmapSet => beatmapSet.ID == importedSet!.ID);
        }

        public static async Task<RealmBeatmapSet> LoadOszIntoStore(BeatmapImporter importer, Realm realm, string? path = null, bool virtualTrack = false)
        {
            string? temp = path ?? TestResources.GetTestBeatmapForImport(virtualTrack);

            var importedSet = await importer.Import(new ImportTask(temp));

            Assert.NotNull(importedSet);
            Debug.Assert(importedSet != null);

            ensureLoaded(realm);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return realm.All<RealmBeatmapSet>().First(beatmapSet => beatmapSet.ID == importedSet.ID);
        }

        private void deleteBeatmapSet(RealmBeatmapSet imported, Realm realm)
        {
            realm.Write(() => imported.DeletePending = true);

            checkBeatmapSetCount(realm, 0);
            checkBeatmapSetCount(realm, 1, true);

            Assert.IsTrue(realm.All<RealmBeatmapSet>().First(_ => true).DeletePending);
        }

        private static Task createScoreForBeatmap(Realm realm, RealmBeatmap beatmap)
        {
            // TODO: reimplement when we have score support in realm.
            // return ImportScoreTest.LoadScoreIntoOsu(osu, new ScoreInfo
            // {
            //     OnlineScoreID = 2,
            //     Beatmap = beatmap,
            //     BeatmapInfoID = beatmap.ID
            // }, new ImportScoreTest.TestArchiveReader());

            return Task.CompletedTask;
        }

        private static void checkBeatmapSetCount(Realm realm, int expected, bool includeDeletePending = false)
        {
            Assert.AreEqual(expected, includeDeletePending
                ? realm.All<RealmBeatmapSet>().Count()
                : realm.All<RealmBeatmapSet>().Count(s => !s.DeletePending));
        }

        private static string hashFile(string filename)
        {
            using (var s = File.OpenRead(filename))
                return s.ComputeMD5Hash();
        }

        private static void checkBeatmapCount(Realm realm, int expected)
        {
            Assert.AreEqual(expected, realm.All<RealmBeatmap>().Where(_ => true).ToList().Count);
        }

        private static void checkSingleReferencedFileCount(Realm realm, int expected)
        {
            int singleReferencedCount = 0;

            foreach (var f in realm.All<RealmFile>())
            {
                if (f.BacklinksCount == 1)
                    singleReferencedCount++;
            }

            Assert.AreEqual(expected, singleReferencedCount);
        }

        private static void ensureLoaded(Realm realm, int timeout = 60000)
        {
            IQueryable<RealmBeatmapSet>? resultSets = null;

            waitForOrAssert(() => (resultSets = realm.All<RealmBeatmapSet>().Where(s => !s.DeletePending && s.OnlineID == 241526)).Any(),
                @"BeatmapSet did not import to the database in allocated time.", timeout);

            // ensure we were stored to beatmap database backing...
            Assert.IsTrue(resultSets?.Count() == 1, $@"Incorrect result count found ({resultSets?.Count()} but should be 1).");

            IEnumerable<RealmBeatmapSet> queryBeatmapSets() => realm.All<RealmBeatmapSet>().Where(s => !s.DeletePending && s.OnlineID == 241526);

            var set = queryBeatmapSets().First();

            // ReSharper disable once PossibleUnintendedReferenceComparison
            IEnumerable<RealmBeatmap> queryBeatmaps() => realm.All<RealmBeatmap>().Where(s => s.BeatmapSet != null && s.BeatmapSet == set);

            waitForOrAssert(() => queryBeatmaps().Count() == 12, @"Beatmaps did not import to the database in allocated time", timeout);
            waitForOrAssert(() => queryBeatmapSets().Count() == 1, @"BeatmapSet did not import to the database in allocated time", timeout);

            int countBeatmapSetBeatmaps = 0;
            int countBeatmaps = 0;

            waitForOrAssert(() =>
                    (countBeatmapSetBeatmaps = queryBeatmapSets().First().Beatmaps.Count) ==
                    (countBeatmaps = queryBeatmaps().Count()),
                $@"Incorrect database beatmap count post-import ({countBeatmaps} but should be {countBeatmapSetBeatmaps}).", timeout);

            foreach (RealmBeatmap b in set.Beatmaps)
                Assert.IsTrue(set.Beatmaps.Any(c => c.OnlineID == b.OnlineID));
            Assert.IsTrue(set.Beatmaps.Count > 0);
        }

        private static void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            const int sleep = 200;

            while (timeout > 0)
            {
                Thread.Sleep(sleep);
                timeout -= sleep;

                if (result())
                    return;
            }

            Assert.Fail(failureMessage);
        }
    }
}
