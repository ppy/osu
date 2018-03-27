// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class ImportBeatmapTest
    {
        private const string osz_path = @"../../../../osu-resources/osu.Game.Resources/Beatmaps/241526 Soleily - Renatus.osz";

        [Test]
        public void TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportWhenClosed"))
            {
                try
                {
                    loadOszIntoOsu(loadOsu(host));
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

                    var imported = loadOszIntoOsu(osu);

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

                    var imported = loadOszIntoOsu(osu);
                    var importedSecondTime = loadOszIntoOsu(osu);

                    // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                    Assert.IsTrue(imported.ID == importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID == importedSecondTime.Beatmaps.First().ID);

                    var manager = osu.Dependencies.Get<BeatmapManager>();

                    Assert.IsTrue(manager.GetAllUsableBeatmapSets().Count == 1);
                    Assert.IsTrue(manager.QueryBeatmapSets(_ => true).ToList().Count == 1);
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

                    var imported = loadOszIntoOsu(osu);

                    //var change = manager.QueryBeatmapSets(_ => true).First();
                    imported.Hash += "-changed";
                    manager.Update(imported);

                    var importedSecondTime = loadOszIntoOsu(osu);

                    // check the newly "imported" beatmap is actually just the restored previous import. since it matches hash.
                    Assert.IsTrue(imported.ID != importedSecondTime.ID);
                    Assert.IsTrue(imported.Beatmaps.First().ID < importedSecondTime.Beatmaps.First().ID);

                    Assert.IsTrue(manager.GetAllUsableBeatmapSets().Count == 1);
                    Assert.IsTrue(manager.QueryBeatmapSets(_ => true).ToList().Count == 1);
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

                    var imported = loadOszIntoOsu(osu);

                    deleteBeatmapSet(imported, osu);

                    var importedSecondTime = loadOszIntoOsu(osu);

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

                    var temp = prepareTempCopy(osz_path);
                    Assert.IsTrue(File.Exists(temp));

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
                    var temp = prepareTempCopy(osz_path);
                    Assert.IsTrue(File.Exists(temp), "Temporary file copy never substantiated");
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

        private BeatmapSetInfo loadOszIntoOsu(OsuGameBase osu)
        {
            var temp = prepareTempCopy(osz_path);

            Assert.IsTrue(File.Exists(temp));

            var manager = osu.Dependencies.Get<BeatmapManager>();

            manager.Import(temp);

            var imported = manager.GetAllUsableBeatmapSets();

            ensureLoaded(osu);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return imported.FirstOrDefault();
        }

        private void deleteBeatmapSet(BeatmapSetInfo imported, OsuGameBase osu)
        {
            var manager = osu.Dependencies.Get<BeatmapManager>();
            manager.Delete(imported);

            Assert.IsTrue(manager.GetAllUsableBeatmapSets().Count == 0);
            Assert.IsTrue(manager.QueryBeatmapSets(_ => true).ToList().Count == 1);
            Assert.IsTrue(manager.QueryBeatmapSets(_ => true).First().DeletePending);
        }

        private string prepareTempCopy(string path)
        {
            var temp = Path.GetTempFileName();
            return new FileInfo(path).CopyTo(temp, true).FullName;
        }

        private OsuGameBase loadOsu(GameHost host)
        {
            var osu = new OsuGameBase();
            Task.Run(() => host.Run(osu));
            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");
            return osu;
        }

        private void ensureLoaded(OsuGameBase osu, int timeout = 60000)
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
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 1))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 2))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 3))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);
        }

        private void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Action waitAction = () =>
            {
                while (!result()) Thread.Sleep(200);
            };

            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout), failureMessage);
        }
    }
}
