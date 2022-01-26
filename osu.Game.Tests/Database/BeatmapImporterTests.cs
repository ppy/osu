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
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
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
        public void TestDetachBeatmapSet()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using (var importer = new BeatmapModelManager(realm, storage))
                using (new RulesetStore(realm, storage))
                {
                    Live<BeatmapSetInfo>? beatmapSet;

                    using (var reader = new ZipArchiveReader(TestResources.GetTestBeatmapStream()))
                        beatmapSet = await importer.Import(reader);

                    Assert.NotNull(beatmapSet);
                    Debug.Assert(beatmapSet != null);

                    BeatmapSetInfo? detachedBeatmapSet = null;

                    beatmapSet.PerformRead(live =>
                    {
                        detachedBeatmapSet = live.Detach();

                        // files are omitted
                        Assert.AreEqual(0, detachedBeatmapSet.Files.Count);

                        Assert.AreEqual(live.Beatmaps.Count, detachedBeatmapSet.Beatmaps.Count);
                        Assert.AreEqual(live.Beatmaps.Select(f => f.Difficulty).Count(), detachedBeatmapSet.Beatmaps.Select(f => f.Difficulty).Count());
                        Assert.AreEqual(live.Metadata, detachedBeatmapSet.Metadata);
                    });

                    Debug.Assert(detachedBeatmapSet != null);

                    // Check detached instances can all be accessed without throwing.
                    Assert.AreEqual(0, detachedBeatmapSet.Files.Count);
                    Assert.NotNull(detachedBeatmapSet.Beatmaps.Count);
                    Assert.NotZero(detachedBeatmapSet.Beatmaps.Select(f => f.Difficulty).Count());
                    Assert.NotNull(detachedBeatmapSet.Metadata);

                    // Check cyclic reference to beatmap set
                    Assert.AreEqual(detachedBeatmapSet, detachedBeatmapSet.Beatmaps.First().BeatmapSet);
                }
            });
        }

        [Test]
        public void TestUpdateDetachedBeatmapSet()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using (var importer = new BeatmapModelManager(realm, storage))
                using (new RulesetStore(realm, storage))
                {
                    Live<BeatmapSetInfo>? beatmapSet;

                    using (var reader = new ZipArchiveReader(TestResources.GetTestBeatmapStream()))
                        beatmapSet = await importer.Import(reader);

                    Assert.NotNull(beatmapSet);
                    Debug.Assert(beatmapSet != null);

                    // Detach at the BeatmapInfo point, similar to what GetWorkingBeatmap does.
                    BeatmapInfo? detachedBeatmap = null;

                    beatmapSet.PerformRead(s => detachedBeatmap = s.Beatmaps.First().Detach());

                    BeatmapSetInfo? detachedBeatmapSet = detachedBeatmap?.BeatmapSet;

                    Debug.Assert(detachedBeatmapSet != null);

                    var newUser = new RealmUser { Username = "peppy", OnlineID = 2 };

                    detachedBeatmapSet.Beatmaps.First().Metadata.Artist = "New Artist";
                    detachedBeatmapSet.Beatmaps.First().Metadata.Author = newUser;

                    Assert.AreNotEqual(detachedBeatmapSet.Status, BeatmapOnlineStatus.Ranked);
                    detachedBeatmapSet.Status = BeatmapOnlineStatus.Ranked;

                    beatmapSet.PerformWrite(s =>
                    {
                        detachedBeatmapSet.CopyChangesToRealm(s);
                    });

                    beatmapSet.PerformRead(s =>
                    {
                        // Check above changes explicitly.
                        Assert.AreEqual(BeatmapOnlineStatus.Ranked, s.Status);
                        Assert.AreEqual("New Artist", s.Beatmaps.First().Metadata.Artist);
                        Assert.AreEqual(newUser, s.Beatmaps.First().Metadata.Author);
                        Assert.NotZero(s.Files.Count);

                        // Check nothing was lost in the copy operation.
                        Assert.AreEqual(s.Files.Count, detachedBeatmapSet.Files.Count);
                        Assert.AreEqual(s.Files.Select(f => f.File).Count(), detachedBeatmapSet.Files.Select(f => f.File).Count());
                        Assert.AreEqual(s.Beatmaps.Count, detachedBeatmapSet.Beatmaps.Count);
                        Assert.AreEqual(s.Beatmaps.Select(f => f.Difficulty).Count(), detachedBeatmapSet.Beatmaps.Select(f => f.Difficulty).Count());
                        Assert.AreEqual(s.Metadata, detachedBeatmapSet.Metadata);
                    });
                }
            });
        }

        [Test]
        public void TestImportBeatmapThenCleanup()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using (var importer = new BeatmapModelManager(realm, storage))
                using (new RulesetStore(realm, storage))
                {
                    Live<BeatmapSetInfo>? imported;

                    using (var reader = new ZipArchiveReader(TestResources.GetTestBeatmapStream()))
                        imported = await importer.Import(reader);

                    Assert.AreEqual(1, realm.Realm.All<BeatmapSetInfo>().Count());

                    Assert.NotNull(imported);
                    Debug.Assert(imported != null);

                    imported.PerformWrite(s => s.DeletePending = true);

                    Assert.AreEqual(1, realm.Realm.All<BeatmapSetInfo>().Count(s => s.DeletePending));
                }
            });

            Logger.Log("Running with no work to purge pending deletions");

            RunTestWithRealm((realm, _) => { Assert.AreEqual(0, realm.Realm.All<BeatmapSetInfo>().Count()); });
        }

        [Test]
        public void TestImportWhenClosed()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                await LoadOszIntoStore(importer, realm.Realm);
            });
        }

        [Test]
        public void TestAccessFileAfterImport()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                var beatmap = imported.Beatmaps.First();
                var file = beatmap.File;

                Assert.NotNull(file);
                Assert.AreEqual(beatmap.Hash, file!.File.Hash);
            });
        }

        [Test]
        public void TestImportThenDelete()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                deleteBeatmapSet(imported, realm.Realm);
            });
        }

        [Test]
        public void TestImportThenDeleteFromStream()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? tempPath = TestResources.GetTestBeatmapForImport();

                Live<BeatmapSetInfo>? importedSet;

                using (var stream = File.OpenRead(tempPath))
                {
                    importedSet = await importer.Import(new ImportTask(stream, Path.GetFileName(tempPath)));
                    EnsureLoaded(realm.Realm);
                }

                Assert.NotNull(importedSet);
                Debug.Assert(importedSet != null);

                Assert.IsTrue(File.Exists(tempPath), "Stream source file somehow went missing");
                File.Delete(tempPath);

                var imported = realm.Realm.All<BeatmapSetInfo>().First(beatmapSet => beatmapSet.ID == importedSet.ID);

                deleteBeatmapSet(imported, realm.Realm);
            });
        }

        [Test]
        public void TestImportThenImport()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);
                var importedSecondTime = await LoadOszIntoStore(importer, realm.Realm);

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                checkBeatmapSetCount(realm.Realm, 1);
                checkSingleReferencedFileCount(realm.Realm, 18);
            });
        }

        [Test]
        public void TestImportThenImportWithReZip()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realm.Realm);

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

                    EnsureLoaded(realm.Realm);

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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realm.Realm);

                    await createScoreForBeatmap(realm.Realm, imported.Beatmaps.First());

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

                    EnsureLoaded(realm.Realm);

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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realm.Realm);

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

                    EnsureLoaded(realm.Realm);

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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? temp = TestResources.GetTestBeatmapForImport();

                string extractedFolder = $"{temp}_extracted";
                Directory.CreateDirectory(extractedFolder);

                try
                {
                    var imported = await LoadOszIntoStore(importer, realm.Realm);

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

                    EnsureLoaded(realm.Realm);

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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                var firstFile = imported.Files.First();

                long originalLength;
                using (var stream = storage.GetStream(firstFile.File.GetStoragePath()))
                    originalLength = stream.Length;

                using (var stream = storage.GetStream(firstFile.File.GetStoragePath(), FileAccess.Write, FileMode.Create))
                    stream.WriteByte(0);

                var importedSecondTime = await LoadOszIntoStore(importer, realm.Realm);

                using (var stream = storage.GetStream(firstFile.File.GetStoragePath()))
                    Assert.AreEqual(stream.Length, originalLength, "Corruption was not fixed on second import");

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                checkBeatmapSetCount(realm.Realm, 1);
                checkSingleReferencedFileCount(realm.Realm, 18);
            });
        }

        [Test]
        public void TestModelCreationFailureDoesntReturn()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var progressNotification = new ImportProgressNotification();

                var zipStream = new MemoryStream();

                using (var zip = ZipArchive.Create())
                    zip.SaveTo(zipStream, new ZipWriterOptions(CompressionType.Deflate));

                var imported = await importer.Import(
                    progressNotification,
                    new ImportTask(zipStream, string.Empty)
                );

                checkBeatmapSetCount(realm.Realm, 0);
                checkBeatmapCount(realm.Realm, 0);

                Assert.IsEmpty(imported);
                Assert.AreEqual(ProgressNotificationState.Cancelled, progressNotification.State);
            });
        }

        [Test]
        public void TestRollbackOnFailure()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                int loggedExceptionCount = 0;

                Logger.NewEntry += l =>
                {
                    if (l.Target == LoggingTarget.Database && l.Exception != null)
                        Interlocked.Increment(ref loggedExceptionCount);
                };

                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                realm.Realm.Write(() => imported.Hash += "-changed");

                checkBeatmapSetCount(realm.Realm, 1);
                checkBeatmapCount(realm.Realm, 12);
                checkSingleReferencedFileCount(realm.Realm, 18);

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

                checkBeatmapSetCount(realm.Realm, 1);
                checkBeatmapCount(realm.Realm, 12);

                checkSingleReferencedFileCount(realm.Realm, 18);

                Assert.AreEqual(1, loggedExceptionCount);

                File.Delete(brokenTempFilename);
            });
        }

        [Test]
        public void TestImportThenDeleteThenImportOptimisedPath()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                deleteBeatmapSet(imported, realm.Realm);

                Assert.IsTrue(imported.DeletePending);

                var importedSecondTime = await LoadOszIntoStore(importer, realm.Realm);

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);
                Assert.IsFalse(imported.DeletePending);
                Assert.IsFalse(importedSecondTime.DeletePending);
            });
        }

        [Test]
        public void TestImportThenReimportAfterMissingFiles()
        {
            RunTestWithRealmAsync(async (realmFactory, storage) =>
            {
                using var importer = new BeatmapModelManager(realmFactory, storage);
                using var store = new RulesetStore(realmFactory, storage);

                var imported = await LoadOszIntoStore(importer, realmFactory.Realm);

                deleteBeatmapSet(imported, realmFactory.Realm);

                Assert.IsTrue(imported.DeletePending);

                // intentionally nuke all files
                storage.DeleteDirectory("files");

                Assert.That(imported.Files.All(f => !storage.GetStorageForDirectory("files").Exists(f.File.GetStoragePath())));

                var importedSecondTime = await LoadOszIntoStore(importer, realmFactory.Realm);

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);
                Assert.IsFalse(imported.DeletePending);
                Assert.IsFalse(importedSecondTime.DeletePending);

                // check that the files now exist, even though they were deleted above.
                Assert.That(importedSecondTime.Files.All(f => storage.GetStorageForDirectory("files").Exists(f.File.GetStoragePath())));
            });
        }

        [Test]
        public void TestImportThenDeleteThenImportNonOptimisedPath()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new NonOptimisedBeatmapImporter(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                deleteBeatmapSet(imported, realm.Realm);

                Assert.IsTrue(imported.DeletePending);

                var importedSecondTime = await LoadOszIntoStore(importer, realm.Realm);

                // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                Assert.IsTrue(imported.ID == importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);
                Assert.IsFalse(imported.DeletePending);
                Assert.IsFalse(importedSecondTime.DeletePending);
            });
        }

        [Test]
        public void TestImportThenDeleteThenImportWithOnlineIDsMissing()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var imported = await LoadOszIntoStore(importer, realm.Realm);

                realm.Realm.Write(() =>
                {
                    foreach (var b in imported.Beatmaps)
                        b.OnlineID = -1;
                });

                deleteBeatmapSet(imported, realm.Realm);

                var importedSecondTime = await LoadOszIntoStore(importer, realm.Realm);

                // check the newly "imported" beatmap has been reimported due to mismatch (even though hashes matched)
                Assert.IsTrue(imported.ID != importedSecondTime.ID);
                Assert.IsTrue(imported.Beatmaps.First().ID != importedSecondTime.Beatmaps.First().ID);
            });
        }

        [Test]
        public void TestImportWithDuplicateBeatmapIDs()
        {
            RunTestWithRealm((realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                var metadata = new BeatmapMetadata
                {
                    Artist = "SomeArtist",
                    Author =
                    {
                        Username = "SomeAuthor"
                    }
                };

                var ruleset = realm.Realm.All<RulesetInfo>().First();

                var toImport = new BeatmapSetInfo
                {
                    OnlineID = 1,
                    Beatmaps =
                    {
                        new BeatmapInfo(ruleset, new BeatmapDifficulty(), metadata)
                        {
                            OnlineID = 2,
                        },
                        new BeatmapInfo(ruleset, new BeatmapDifficulty(), metadata)
                        {
                            OnlineID = 2,
                            Status = BeatmapOnlineStatus.Loved,
                        }
                    }
                };

                var imported = importer.Import(toImport);

                Assert.NotNull(imported);
                Debug.Assert(imported != null);

                Assert.AreEqual(-1, imported.PerformRead(s => s.Beatmaps[0].OnlineID));
                Assert.AreEqual(-1, imported.PerformRead(s => s.Beatmaps[1].OnlineID));
            });
        }

        [Test]
        public void TestImportWhenFileOpen()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? temp = TestResources.GetTestBeatmapForImport();
                using (File.OpenRead(temp))
                    await importer.Import(temp);
                EnsureLoaded(realm.Realm);
                File.Delete(temp);
                Assert.IsFalse(File.Exists(temp), "We likely held a read lock on the file when we shouldn't");
            });
        }

        [Test]
        public void TestImportWithDuplicateHashes()
        {
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

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

                    EnsureLoaded(realm.Realm);
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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

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

                    EnsureLoaded(realm.Realm);

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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

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

                    EnsureLoaded(realm.Realm);

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
            RunTestWithRealmAsync(async (realm, storage) =>
            {
                using var importer = new BeatmapModelManager(realm, storage);
                using var store = new RulesetStore(realm, storage);

                string? temp = TestResources.GetTestBeatmapForImport();
                await importer.Import(temp);

                // Update via the beatmap, not the beatmap info, to ensure correct linking
                BeatmapSetInfo setToUpdate = realm.Realm.All<BeatmapSetInfo>().First();

                var beatmapToUpdate = setToUpdate.Beatmaps.First();

                realm.Realm.Write(() => beatmapToUpdate.DifficultyName = "updated");

                BeatmapInfo updatedInfo = realm.Realm.All<BeatmapInfo>().First(b => b.ID == beatmapToUpdate.ID);
                Assert.That(updatedInfo.DifficultyName, Is.EqualTo("updated"));
            });
        }

        public static async Task<BeatmapSetInfo?> LoadQuickOszIntoOsu(BeatmapImporter importer, Realm realm)
        {
            string? temp = TestResources.GetQuickTestBeatmapForImport();

            var importedSet = await importer.Import(new ImportTask(temp));

            Assert.NotNull(importedSet);

            EnsureLoaded(realm);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return realm.All<BeatmapSetInfo>().FirstOrDefault(beatmapSet => beatmapSet.ID == importedSet!.ID);
        }

        public static async Task<BeatmapSetInfo> LoadOszIntoStore(BeatmapImporter importer, Realm realm, string? path = null, bool virtualTrack = false)
        {
            string? temp = path ?? TestResources.GetTestBeatmapForImport(virtualTrack);

            var importedSet = await importer.Import(new ImportTask(temp));

            Assert.NotNull(importedSet);
            Debug.Assert(importedSet != null);

            EnsureLoaded(realm);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return realm.All<BeatmapSetInfo>().First(beatmapSet => beatmapSet.ID == importedSet.ID);
        }

        private void deleteBeatmapSet(BeatmapSetInfo imported, Realm realm)
        {
            realm.Write(() => imported.DeletePending = true);

            checkBeatmapSetCount(realm, 0);
            checkBeatmapSetCount(realm, 1, true);

            Assert.IsTrue(realm.All<BeatmapSetInfo>().First(_ => true).DeletePending);
        }

        private static Task createScoreForBeatmap(Realm realm, BeatmapInfo beatmap)
        {
            // TODO: reimplement when we have score support in realm.
            // return ImportScoreTest.LoadScoreIntoOsu(osu, new ScoreInfo
            // {
            //     OnlineID = 2,
            //     Beatmap = beatmap,
            //     BeatmapInfoID = beatmap.ID
            // }, new ImportScoreTest.TestArchiveReader());

            return Task.CompletedTask;
        }

        private static void checkBeatmapSetCount(Realm realm, int expected, bool includeDeletePending = false)
        {
            Assert.AreEqual(expected, includeDeletePending
                ? realm.All<BeatmapSetInfo>().Count()
                : realm.All<BeatmapSetInfo>().Count(s => !s.DeletePending));
        }

        private static string hashFile(string filename)
        {
            using (var s = File.OpenRead(filename))
                return s.ComputeMD5Hash();
        }

        private static void checkBeatmapCount(Realm realm, int expected)
        {
            Assert.AreEqual(expected, realm.All<BeatmapInfo>().Where(_ => true).ToList().Count);
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

        internal static void EnsureLoaded(Realm realm, int timeout = 60000)
        {
            IQueryable<BeatmapSetInfo>? resultSets = null;

            waitForOrAssert(() =>
            {
                realm.Refresh();
                return (resultSets = realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending && s.OnlineID == 241526)).Any();
            }, @"BeatmapSet did not import to the database in allocated time.", timeout);

            // ensure we were stored to beatmap database backing...
            Assert.IsTrue(resultSets?.Count() == 1, $@"Incorrect result count found ({resultSets?.Count()} but should be 1).");

            IEnumerable<BeatmapSetInfo> queryBeatmapSets() => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending && s.OnlineID == 241526);

            var set = queryBeatmapSets().First();

            // ReSharper disable once PossibleUnintendedReferenceComparison
            IEnumerable<BeatmapInfo> queryBeatmaps() => realm.All<BeatmapInfo>().Where(s => s.BeatmapSet != null && s.BeatmapSet == set);

            Assert.AreEqual(12, queryBeatmaps().Count(), @"Beatmap count was not correct");
            Assert.AreEqual(1, queryBeatmapSets().Count(), @"Beatmapset count was not correct");

            int countBeatmapSetBeatmaps;
            int countBeatmaps;

            Assert.AreEqual(
                countBeatmapSetBeatmaps = queryBeatmapSets().First().Beatmaps.Count,
                countBeatmaps = queryBeatmaps().Count(),
                $@"Incorrect database beatmap count post-import ({countBeatmaps} but should be {countBeatmapSetBeatmaps}).");

            foreach (BeatmapInfo b in set.Beatmaps)
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

        public class NonOptimisedBeatmapImporter : BeatmapImporter
        {
            public NonOptimisedBeatmapImporter(RealmAccess realm, Storage storage)
                : base(realm, storage)
            {
            }

            protected override bool HasCustomHashFunction => true;
        }
    }
}
