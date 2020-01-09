// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Game.IPC;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Resources;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class ImportBeatmapTest
    {
        [Test]
        public async Task TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportWhenClosed)))
            {
                try
                {
                    await LoadOszIntoOsu(loadOsu(host));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportThenDelete()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportThenDelete)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = await LoadOszIntoOsu(osu);

                    deleteBeatmapSet(imported, osu);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportThenImport()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportThenImport)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = await LoadOszIntoOsu(osu);
                    var importedSecondTime = await LoadOszIntoOsu(osu);

                    // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                    Assert.IsTrue(imported.ID == importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                    checkBeatmapSetCount(osu, 1);
                    checkSingleReferencedFileCount(osu, 18);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportCorruptThenImport()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportCorruptThenImport)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = await LoadOszIntoOsu(osu);

                    var firstFile = imported.Files.First();

                    var files = osu.Dependencies.Get<FileStore>();

                    long originalLength;
                    using (var stream = files.Storage.GetStream(firstFile.FileInfo.StoragePath))
                        originalLength = stream.Length;

                    using (var stream = files.Storage.GetStream(firstFile.FileInfo.StoragePath, FileAccess.Write, FileMode.Create))
                        stream.WriteByte(0);

                    var importedSecondTime = await LoadOszIntoOsu(osu);

                    using (var stream = files.Storage.GetStream(firstFile.FileInfo.StoragePath))
                        Assert.AreEqual(stream.Length, originalLength, "Corruption was not fixed on second import");

                    // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                    Assert.IsTrue(imported.ID == importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                    checkBeatmapSetCount(osu, 1);
                    checkSingleReferencedFileCount(osu, 18);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestRollbackOnFailure()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestRollbackOnFailure)))
            {
                try
                {
                    int itemAddRemoveFireCount = 0;
                    int loggedExceptionCount = 0;

                    Logger.NewEntry += l =>
                    {
                        if (l.Target == LoggingTarget.Database && l.Exception != null)
                            Interlocked.Increment(ref loggedExceptionCount);
                    };

                    var osu = loadOsu(host);
                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    // ReSharper disable once AccessToModifiedClosure
                    manager.ItemAdded += _ => Interlocked.Increment(ref itemAddRemoveFireCount);
                    manager.ItemRemoved += _ => Interlocked.Increment(ref itemAddRemoveFireCount);

                    var imported = await LoadOszIntoOsu(osu);

                    Assert.AreEqual(0, itemAddRemoveFireCount -= 1);

                    imported.Hash += "-changed";
                    manager.Update(imported);

                    Assert.AreEqual(0, itemAddRemoveFireCount -= 2);

                    checkBeatmapSetCount(osu, 1);
                    checkBeatmapCount(osu, 12);
                    checkSingleReferencedFileCount(osu, 18);

                    var breakTemp = TestResources.GetTestBeatmapForImport();

                    MemoryStream brokenOsu = new MemoryStream();
                    MemoryStream brokenOsz = new MemoryStream(File.ReadAllBytes(breakTemp));

                    File.Delete(breakTemp);

                    using (var outStream = File.Open(breakTemp, FileMode.CreateNew))
                    using (var zip = ZipArchive.Open(brokenOsz))
                    {
                        zip.AddEntry("broken.osu", brokenOsu, false);
                        zip.SaveTo(outStream, CompressionType.Deflate);
                    }

                    // this will trigger purging of the existing beatmap (online set id match) but should rollback due to broken osu.
                    try
                    {
                        await manager.Import(breakTemp);
                    }
                    catch
                    {
                    }

                    // no events should be fired in the case of a rollback.
                    Assert.AreEqual(0, itemAddRemoveFireCount);

                    checkBeatmapSetCount(osu, 1);
                    checkBeatmapCount(osu, 12);

                    checkSingleReferencedFileCount(osu, 18);

                    Assert.AreEqual(1, loggedExceptionCount);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportThenImportDifferentHash()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportThenImportDifferentHash)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    var imported = await LoadOszIntoOsu(osu);

                    imported.Hash += "-changed";
                    manager.Update(imported);

                    var importedSecondTime = await LoadOszIntoOsu(osu);

                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID < importedSecondTime.Beatmaps.First().ID);

                    // only one beatmap will exist as the online set ID matched, causing purging of the first import.
                    checkBeatmapSetCount(osu, 1);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportThenDeleteThenImport()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportThenDeleteThenImport)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = await LoadOszIntoOsu(osu);

                    deleteBeatmapSet(imported, osu);

                    var importedSecondTime = await LoadOszIntoOsu(osu);

                    // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                    Assert.IsTrue(imported.ID == importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestImportThenDeleteThenImportWithOnlineIDMismatch(bool set)
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost($"{nameof(TestImportThenDeleteThenImportWithOnlineIDMismatch)}-{set}"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = await LoadOszIntoOsu(osu);

                    if (set)
                        imported.OnlineBeatmapSetID = 1234;
                    else
                        imported.Beatmaps.First().OnlineBeatmapID = 1234;

                    osu.Dependencies.Get<BeatmapManager>().Update(imported);

                    deleteBeatmapSet(imported, osu);

                    var importedSecondTime = await LoadOszIntoOsu(osu);

                    // check the newly "imported" beatmap has been reimported due to mismatch (even though hashes matched)
                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID != importedSecondTime.Beatmaps.First().ID);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportWithDuplicateBeatmapIDs()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportWithDuplicateBeatmapIDs)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var metadata = new BeatmapMetadata
                    {
                        Artist = "SomeArtist",
                        AuthorString = "SomeAuthor"
                    };

                    var difficulty = new BeatmapDifficulty();

                    var toImport = new BeatmapSetInfo
                    {
                        OnlineBeatmapSetID = 1,
                        Metadata = metadata,
                        Beatmaps = new List<BeatmapInfo>
                        {
                            new BeatmapInfo
                            {
                                OnlineBeatmapID = 2,
                                Metadata = metadata,
                                BaseDifficulty = difficulty
                            },
                            new BeatmapInfo
                            {
                                OnlineBeatmapID = 2,
                                Metadata = metadata,
                                Status = BeatmapSetOnlineStatus.Loved,
                                BaseDifficulty = difficulty
                            }
                        }
                    };

                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    var imported = await manager.Import(toImport);

                    Assert.NotNull(imported);
                    Assert.AreEqual(null, imported.Beatmaps[0].OnlineBeatmapID);
                    Assert.AreEqual(null, imported.Beatmaps[1].OnlineBeatmapID);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        [NonParallelizable]
        [Ignore("Binding IPC on Appveyor isn't working (port in use). Need to figure out why")]
        public void TestImportOverIPC()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("host", true))
            using (HeadlessGameHost client = new CleanRunHeadlessGameHost("client", true))
            {
                try
                {
                    Assert.IsTrue(host.IsPrimaryInstance);
                    Assert.IsFalse(client.IsPrimaryInstance);

                    var osu = loadOsu(host);

                    var temp = TestResources.GetTestBeatmapForImport();

                    var importer = new ArchiveImportIPCChannel(client);
                    if (!importer.ImportAsync(temp).Wait(10000))
                        Assert.Fail(@"IPC took too long to send");

                    ensureLoaded(osu);

                    waitForOrAssert(() => !File.Exists(temp), "Temporary still exists after IPC import", 5000);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportWhenFileOpen()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportWhenFileOpen)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var temp = TestResources.GetTestBeatmapForImport();
                    using (File.OpenRead(temp))
                        await osu.Dependencies.Get<BeatmapManager>().Import(temp);
                    ensureLoaded(osu);
                    File.Delete(temp);
                    Assert.IsFalse(File.Exists(temp), "We likely held a read lock on the file when we shouldn't");
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportWithDuplicateHashes()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportNestedStructure)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var temp = TestResources.GetTestBeatmapForImport();

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

                        await osu.Dependencies.Get<BeatmapManager>().Import(temp);

                        ensureLoaded(osu);
                    }
                    finally
                    {
                        Directory.Delete(extractedFolder, true);
                    }
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportNestedStructure()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportNestedStructure)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var temp = TestResources.GetTestBeatmapForImport();

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

                        var imported = await osu.Dependencies.Get<BeatmapManager>().Import(temp);

                        ensureLoaded(osu);

                        Assert.IsFalse(imported.Files.Any(f => f.Filename.Contains("subfolder")), "Files contain common subfolder");
                    }
                    finally
                    {
                        Directory.Delete(extractedFolder, true);
                    }
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportWithIgnoredDirectoryInArchive()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestImportWithIgnoredDirectoryInArchive)))
            {
                try
                {
                    var osu = loadOsu(host);

                    var temp = TestResources.GetTestBeatmapForImport();

                    string extractedFolder = $"{temp}_extracted";
                    string dataFolder = Path.Combine(extractedFolder, "actual_data");
                    string resourceForkFolder = Path.Combine(extractedFolder, "__MACOSX");
                    string resourceForkFilePath = Path.Combine(resourceForkFolder, ".extracted");

                    Directory.CreateDirectory(dataFolder);
                    Directory.CreateDirectory(resourceForkFolder);

                    using (var resourceForkFile = File.CreateText(resourceForkFilePath))
                    {
                        resourceForkFile.WriteLine("adding content so that it's not empty");
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

                        var imported = await osu.Dependencies.Get<BeatmapManager>().Import(temp);

                        ensureLoaded(osu);

                        Assert.IsFalse(imported.Files.Any(f => f.Filename.Contains("__MACOSX")), "Files contain resource fork folder, which should be ignored");
                        Assert.IsFalse(imported.Files.Any(f => f.Filename.Contains("actual_data")), "Files contain common subfolder");
                    }
                    finally
                    {
                        Directory.Delete(extractedFolder, true);
                    }
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestUpdateBeatmapInfoContents()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestUpdateBeatmapInfoContents)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    var temp = TestResources.GetTestBeatmapForImport();
                    await osu.Dependencies.Get<BeatmapManager>().Import(temp);

                    // Update via the beatmap, not the beatmap info, to ensure correct linking
                    BeatmapSetInfo setToUpdate = manager.GetAllUsableBeatmapSets()[0];
                    Beatmap beatmapToUpdate = (Beatmap)manager.GetWorkingBeatmap(setToUpdate.Beatmaps.First(b => b.RulesetID == 0)).Beatmap;
                    beatmapToUpdate.BeatmapInfo.Version = "updated";

                    manager.Update(setToUpdate);

                    BeatmapInfo updatedInfo = manager.QueryBeatmap(b => b.ID == beatmapToUpdate.BeatmapInfo.ID);
                    Assert.That(updatedInfo.Version, Is.EqualTo("updated"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestUpdateBeatmapFileContents()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(TestUpdateBeatmapFileContents)))
            {
                try
                {
                    var osu = loadOsu(host);
                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    var temp = TestResources.GetTestBeatmapForImport();
                    await osu.Dependencies.Get<BeatmapManager>().Import(temp);

                    BeatmapSetInfo setToUpdate = manager.GetAllUsableBeatmapSets()[0];
                    Beatmap beatmapToUpdate = (Beatmap)manager.GetWorkingBeatmap(setToUpdate.Beatmaps.First(b => b.RulesetID == 0)).Beatmap;
                    BeatmapSetFileInfo fileToUpdate = setToUpdate.Files.First(f => beatmapToUpdate.BeatmapInfo.Path.Contains(f.Filename));

                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(stream, leaveOpen: true))
                        {
                            beatmapToUpdate.HitObjects.Clear();
                            beatmapToUpdate.HitObjects.Add(new HitCircle { StartTime = 5000 });

                            new LegacyBeatmapEncoder(beatmapToUpdate).Encode(writer);
                        }

                        stream.Seek(0, SeekOrigin.Begin);

                        manager.UpdateFile(fileToUpdate, stream);
                    }

                    Beatmap updatedBeatmap = (Beatmap)manager.GetWorkingBeatmap(manager.QueryBeatmap(b => b.ID == beatmapToUpdate.BeatmapInfo.ID)).Beatmap;

                    Assert.That(updatedBeatmap.HitObjects.Count, Is.EqualTo(1));
                    Assert.That(updatedBeatmap.HitObjects[0].StartTime, Is.EqualTo(5000));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        public static async Task<BeatmapSetInfo> LoadOszIntoOsu(OsuGameBase osu, string path = null, bool virtualTrack = false)
        {
            var temp = path ?? TestResources.GetTestBeatmapForImport(virtualTrack);

            var manager = osu.Dependencies.Get<BeatmapManager>();

            await manager.Import(temp);

            var imported = manager.GetAllUsableBeatmapSets();

            ensureLoaded(osu);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return imported.LastOrDefault();
        }

        private void deleteBeatmapSet(BeatmapSetInfo imported, OsuGameBase osu)
        {
            var manager = osu.Dependencies.Get<BeatmapManager>();
            manager.Delete(imported);

            checkBeatmapSetCount(osu, 0);
            checkBeatmapSetCount(osu, 1, true);
            checkSingleReferencedFileCount(osu, 0);

            Assert.IsTrue(manager.QueryBeatmapSets(_ => true).First().DeletePending);
        }

        private void checkBeatmapSetCount(OsuGameBase osu, int expected, bool includeDeletePending = false)
        {
            var manager = osu.Dependencies.Get<BeatmapManager>();

            Assert.AreEqual(expected, includeDeletePending
                ? manager.QueryBeatmapSets(_ => true).ToList().Count
                : manager.GetAllUsableBeatmapSets().Count);
        }

        private void checkBeatmapCount(OsuGameBase osu, int expected)
        {
            Assert.AreEqual(expected, osu.Dependencies.Get<BeatmapManager>().QueryBeatmaps(_ => true).ToList().Count);
        }

        private void checkSingleReferencedFileCount(OsuGameBase osu, int expected)
        {
            Assert.AreEqual(expected, osu.Dependencies.Get<FileStore>().QueryFiles(f => f.ReferenceCount == 1).Count());
        }

        private OsuGameBase loadOsu(GameHost host)
        {
            var osu = new OsuGameBase();
            Task.Run(() => host.Run(osu));
            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");
            return osu;
        }

        private static void ensureLoaded(OsuGameBase osu, int timeout = 60000)
        {
            IEnumerable<BeatmapSetInfo> resultSets = null;
            var store = osu.Dependencies.Get<BeatmapManager>();
            waitForOrAssert(() => (resultSets = store.QueryBeatmapSets(s => s.OnlineBeatmapSetID == 241526)).Any(),
                @"BeatmapSet did not import to the database in allocated time.", timeout);

            //ensure we were stored to beatmap database backing...
            Assert.IsTrue(resultSets.Count() == 1, $@"Incorrect result count found ({resultSets.Count()} but should be 1).");
            IEnumerable<BeatmapInfo> queryBeatmaps() => store.QueryBeatmaps(s => s.BeatmapSet.OnlineBeatmapSetID == 241526 && s.BaseDifficultyID > 0);
            IEnumerable<BeatmapSetInfo> queryBeatmapSets() => store.QueryBeatmapSets(s => s.OnlineBeatmapSetID == 241526);

            //if we don't re-check here, the set will be inserted but the beatmaps won't be present yet.
            waitForOrAssert(() => queryBeatmaps().Count() == 12,
                @"Beatmaps did not import to the database in allocated time", timeout);
            waitForOrAssert(() => queryBeatmapSets().Count() == 1,
                @"BeatmapSet did not import to the database in allocated time", timeout);
            int countBeatmapSetBeatmaps = 0;
            int countBeatmaps = 0;
            waitForOrAssert(() =>
                    (countBeatmapSetBeatmaps = queryBeatmapSets().First().Beatmaps.Count) ==
                    (countBeatmaps = queryBeatmaps().Count()),
                $@"Incorrect database beatmap count post-import ({countBeatmaps} but should be {countBeatmapSetBeatmaps}).", timeout);

            var set = queryBeatmapSets().First();
            foreach (BeatmapInfo b in set.Beatmaps)
                Assert.IsTrue(set.Beatmaps.Any(c => c.OnlineBeatmapID == b.OnlineBeatmapID));
            Assert.IsTrue(set.Beatmaps.Count > 0);
            var beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 0))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 1))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 2))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 3))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
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
