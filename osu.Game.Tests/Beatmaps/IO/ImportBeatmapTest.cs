using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Desktop.Platform;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IPC;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class ImportBeatmapTest
    {
        const string osz_path = @"../../../osu-resources/osu.Game.Resources/Beatmaps/241526 Soleily - Renatus.osz";

        [TestFixtureSetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            HeadlessGameHost host = new HeadlessGameHost();

            var osu = loadOsu(host);
            osu.Beatmaps.Import(osz_path);
            ensureLoaded(osu);
        }

        [Test]
        public void TestImportOverIPC()
        {
            HeadlessGameHost host = new HeadlessGameHost(true);
            HeadlessGameHost client = new HeadlessGameHost(true);

            Assert.IsTrue(host.IsPrimaryInstance);
            Assert.IsTrue(!client.IsPrimaryInstance);

            var osu = loadOsu(host);

            var importer = new BeatmapImporter(client);
            if (!importer.Import(osz_path).Wait(1000))
                    Assert.Fail(@"IPC took too long to send");

            ensureLoaded(osu, 10000);
        }

        private OsuGameBase loadOsu(BasicGameHost host)
        {
            var osu = new OsuGameBase();
            host.Add(osu);

            //reset beatmap database (sqlite and storage backing)
            osu.Beatmaps.Reset();

            return osu;
        }

        private void ensureLoaded(OsuGameBase osu, int timeout = 100)
        {
            IEnumerable<BeatmapSetInfo> resultSets = null;

            Action waitAction = () =>
            {
                while ((resultSets = osu.Beatmaps.Query<BeatmapSetInfo>().Where(s => s.BeatmapSetID == 241526)).Count() != 1)
                    Thread.Sleep(1);
            };

            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout),
                @"BeatmapSet did not import to the database");

            //ensure we were stored to beatmap database backing...
            
            Assert.IsTrue(resultSets.Count() == 1);

            IEnumerable<BeatmapInfo> resultBeatmaps = null;

            //if we don't re-check here, the set will be inserted but the beatmaps won't be present yet.
            waitAction = () =>
            {
                while ((resultBeatmaps = osu.Beatmaps.Query<BeatmapInfo>().Where(s => s.BeatmapSetID == 241526 && s.BaseDifficultyID > 0)).Count() != 12)
                    Thread.Sleep(1);
            };

            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout),
                @"Beatmaps did not import to the database");

            //fetch children and check we can load from the post-storage path...
            var set = osu.Beatmaps.GetChildren(resultSets.First());

            Assert.IsTrue(set.Beatmaps.Count == resultBeatmaps.Count());

            foreach (BeatmapInfo b in resultBeatmaps)
                Assert.IsTrue(set.Beatmaps.Any(c => c.BeatmapID == b.BeatmapID));

            Assert.IsTrue(set.Beatmaps.Count > 0);

            var beatmap = osu.Beatmaps.GetBeatmap(set.Beatmaps[0]);

            Assert.IsTrue(beatmap.HitObjects.Count > 0);
        }
    }
}

