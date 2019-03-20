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
using osu.Game.Beatmaps;
using osu.Game.Tests.Resources;
using SharpCompress.Archives.Zip;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class ImportBeatmapTest
    {
        [Test]
        public void TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportWhenClosed"))
            {
                try
                {
                    LoadOszIntoOsu(loadOsu(host));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestImportThenDelete()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportThenDelete"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = LoadOszIntoOsu(osu);

                    deleteBeatmapSet(imported, osu);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestImportThenImport()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportThenImport"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = LoadOszIntoOsu(osu);
                    var importedSecondTime = LoadOszIntoOsu(osu);

                    // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                    Assert.IsTrue(imported.ID == importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    Assert.AreEqual(1, manager.GetAllUsableBeatmapSets().Count);
                    Assert.AreEqual(1, manager.QueryBeatmapSets(_ => true).ToList().Count);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestRollbackOnFailure()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestRollbackOnFailure"))
            {
                try
                {
                    var osu = loadOsu(host);
                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    int fireCount = 0;

                    // ReSharper disable once AccessToModifiedClosure
                    manager.ItemAdded += (_, __) => fireCount++;
                    manager.ItemRemoved += _ => fireCount++;

                    var imported = LoadOszIntoOsu(osu);

                    Assert.AreEqual(0, fireCount -= 1);

                    imported.Hash += "-changed";
                    manager.Update(imported);

                    Assert.AreEqual(0, fireCount -= 2);

                    var breakTemp = TestResources.GetTestBeatmapForImport();

                    MemoryStream brokenOsu = new MemoryStream(new byte[] { 1, 3, 3, 7 });
                    MemoryStream brokenOsz = new MemoryStream(File.ReadAllBytes(breakTemp));

                    File.Delete(breakTemp);

                    using (var outStream = File.Open(breakTemp, FileMode.CreateNew))
                    using (var zip = ZipArchive.Open(brokenOsz))
                    {
                        zip.AddEntry("broken.osu", brokenOsu, false);
                        zip.SaveTo(outStream, SharpCompress.Common.CompressionType.Deflate);
                    }

                    Assert.AreEqual(1, manager.GetAllUsableBeatmapSets().Count);
                    Assert.AreEqual(1, manager.QueryBeatmapSets(_ => true).ToList().Count);
                    Assert.AreEqual(12, manager.QueryBeatmaps(_ => true).ToList().Count);

                    // this will trigger purging of the existing beatmap (online set id match) but should rollback due to broken osu.
                    manager.Import(breakTemp);

                    // no events should be fired in the case of a rollback.
                    Assert.AreEqual(0, fireCount);

                    Assert.AreEqual(1, manager.GetAllUsableBeatmapSets().Count);
                    Assert.AreEqual(1, manager.QueryBeatmapSets(_ => true).ToList().Count);
                    Assert.AreEqual(12, manager.QueryBeatmaps(_ => true).ToList().Count);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestImportThenImportDifferentHash()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportThenImportDifferentHash"))
            {
                try
                {
                    var osu = loadOsu(host);
                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    var imported = LoadOszIntoOsu(osu);

                    imported.Hash += "-changed";
                    manager.Update(imported);

                    var importedSecondTime = LoadOszIntoOsu(osu);

                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID < importedSecondTime.Beatmaps.First().ID);

                    // only one beatmap will exist as the online set ID matched, causing purging of the first import.
                    Assert.AreEqual(1, manager.GetAllUsableBeatmapSets().Count);
                    Assert.AreEqual(1, manager.QueryBeatmapSets(_ => true).ToList().Count);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestImportThenDeleteThenImport()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportThenDeleteThenImport"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = LoadOszIntoOsu(osu);

                    deleteBeatmapSet(imported, osu);

                    var importedSecondTime = LoadOszIntoOsu(osu);

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
        public void TestImportThenDeleteThenImportWithOnlineIDMismatch(bool set)
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost($"TestImportThenDeleteThenImport-{set}"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var imported = LoadOszIntoOsu(osu);

                    if (set)
                        imported.OnlineBeatmapSetID = 1234;
                    else
                        imported.Beatmaps.First().OnlineBeatmapID = 1234;

                    osu.Dependencies.Get<BeatmapManager>().Update(imported);

                    deleteBeatmapSet(imported, osu);

                    var importedSecondTime = LoadOszIntoOsu(osu);

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
        public void TestImportWithDuplicateBeatmapIDs()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportWithDuplicateBeatmapID"))
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

                    var imported = manager.Import(toImport);

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
        public void TestImportWhenFileOpen()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportWhenFileOpen"))
            {
                try
                {
                    var osu = loadOsu(host);
                    var temp = TestResources.GetTestBeatmapForImport();
                    using (File.OpenRead(temp))
                        osu.Dependencies.Get<BeatmapManager>().Import(temp);
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

        public static BeatmapSetInfo LoadOszIntoOsu(OsuGameBase osu, string path = null)
        {
            var temp = path ?? TestResources.GetTestBeatmapForImport();

            var manager = osu.Dependencies.Get<BeatmapManager>();

            manager.Import(temp);

            var imported = manager.GetAllUsableBeatmapSets();

            ensureLoaded(osu);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return imported.LastOrDefault();
        }

        private void deleteBeatmapSet(BeatmapSetInfo imported, OsuGameBase osu)
        {
            var manager = osu.Dependencies.Get<BeatmapManager>();
            manager.Delete(imported);

            Assert.IsTrue(manager.GetAllUsableBeatmapSets().Count == 0);
            Assert.AreEqual(1, manager.QueryBeatmapSets(_ => true).ToList().Count);
            Assert.IsTrue(manager.QueryBeatmapSets(_ => true).First().DeletePending);
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
