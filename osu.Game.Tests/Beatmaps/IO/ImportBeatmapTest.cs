﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        private const string osz_path = @"../../../osu-resources/osu.Game.Resources/Beatmaps/241526 Soleily - Renatus.osz";

        [Test]
        public void TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new HeadlessGameHost("TestImportWhenClosed"))
            {
                var osu = loadOsu(host);

                var temp = prepareTempCopy(osz_path);

                Assert.IsTrue(File.Exists(temp));

                osu.Dependencies.Get<BeatmapManager>().Import(temp);

                ensureLoaded(osu);

                waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);
            }
        }

        [Test]
        public void TestImportOverIPC()
        {
            using (HeadlessGameHost host = new HeadlessGameHost("host", true))
            using (HeadlessGameHost client = new HeadlessGameHost("client", true))
            {
                Assert.IsTrue(host.IsPrimaryInstance);
                Assert.IsTrue(!client.IsPrimaryInstance);

                var osu = loadOsu(host);

                var temp = prepareTempCopy(osz_path);

                Assert.IsTrue(File.Exists(temp));

                var importer = new BeatmapIPCChannel(client);
                if (!importer.ImportAsync(temp).Wait(10000))
                    Assert.Fail(@"IPC took too long to send");

                ensureLoaded(osu);

                waitForOrAssert(() => !File.Exists(temp), "Temporary still exists after IPC import", 5000);
            }
        }

        [Test]
        public void TestImportWhenFileOpen()
        {
            using (HeadlessGameHost host = new HeadlessGameHost("TestImportWhenFileOpen"))
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
        }

        private string prepareTempCopy(string path)
        {
            var temp = Path.GetTempFileName();
            return new FileInfo(path).CopyTo(temp, true).FullName;
        }

        private OsuGameBase loadOsu(GameHost host)
        {
            host.Storage.DeleteDatabase(@"client");

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

            Func<IEnumerable<BeatmapInfo>> queryBeatmaps = () => store.QueryBeatmaps(s => s.OnlineBeatmapSetID == 241526 && s.BaseDifficultyID > 0);
            Func<IEnumerable<BeatmapSetInfo>> queryBeatmapSets = () => store.QueryBeatmapSets(s => s.OnlineBeatmapSetID == 241526);

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
            Action waitAction = () => { while (!result()) Thread.Sleep(20); };
            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout), failureMessage);
        }
    }
}
