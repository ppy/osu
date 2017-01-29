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
using osu.Game.Modes;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class ImportBeatmapTest
    {
        const string osz_path = @"../../../osu-resources/osu.Game.Resources/Beatmaps/241526 Soleily - Renatus.osz";

        [OneTimeSetUp]
        public void SetUp()
        {
            Ruleset.Register(new OsuRuleset());
            Ruleset.Register(new TaikoRuleset());
            Ruleset.Register(new ManiaRuleset());
            Ruleset.Register(new CatchRuleset());
        }

        [Test]
        public void TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new HeadlessGameHost())
            {
                var osu = loadOsu(host);
                osu.Dependencies.Get<BeatmapDatabase>().Import(osz_path);
                ensureLoaded(osu);
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

                var importer = new BeatmapImporter(client);
                if (!importer.Import(osz_path).Wait(1000))
                    Assert.Fail(@"IPC took too long to send");

                ensureLoaded(osu, 10000);
            }
        }

        private OsuGameBase loadOsu(BasicGameHost host)
        {
            var osu = new OsuGameBase();
            host.Add(osu);

            while (!osu.IsLoaded)
                Thread.Sleep(1);

            //reset beatmap database (sqlite and storage backing)
            osu.Dependencies.Get<BeatmapDatabase>().Reset();

            return osu;
        }

        private void ensureLoaded(OsuGameBase osu, int timeout = 100)
        {
            IEnumerable<BeatmapSetInfo> resultSets = null;

            Action waitAction = () =>
            {
                while ((resultSets = osu.Dependencies.Get<BeatmapDatabase>()
                    .Query<BeatmapSetInfo>().Where(s => s.OnlineBeatmapSetID == 241526)).Count() != 1)
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
                while ((resultBeatmaps = osu.Dependencies.Get<BeatmapDatabase>()
                    .Query<BeatmapInfo>().Where(s => s.OnlineBeatmapSetID == 241526 && s.BaseDifficultyID > 0)).Count() != 12)
                    Thread.Sleep(1);
            };

            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout),
                @"Beatmaps did not import to the database");

            //fetch children and check we can load from the post-storage path...
            var set = osu.Dependencies.Get<BeatmapDatabase>().GetChildren(resultSets.First());

            Assert.IsTrue(set.Beatmaps.Count == resultBeatmaps.Count());

            foreach (BeatmapInfo b in resultBeatmaps)
                Assert.IsTrue(set.Beatmaps.Any(c => c.OnlineBeatmapID == b.OnlineBeatmapID));

            Assert.IsTrue(set.Beatmaps.Count > 0);

            var beatmap = osu.Dependencies.Get<BeatmapDatabase>().GetBeatmap(set.Beatmaps.First(b => b.Mode == PlayMode.Osu));

            Assert.IsTrue(beatmap.HitObjects.Count > 0);
        }
    }
}

