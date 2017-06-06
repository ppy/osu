﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private const string osz_path = @"../../../osu-resources/osu.Game.Resources/Beatmaps/241526 Soleily - Renatus.osz";

        [Test]
        public void TestImportWhenClosed()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new HeadlessGameHost())
            {
                loadOsu(host);

                var temp = prepareTempCopy(osz_path);

                Assert.IsTrue(File.Exists(temp));

                host.Dependencies.Get<BeatmapDatabase>().Import(temp);

                ensureLoaded(host);

                Assert.IsFalse(File.Exists(temp));
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

                loadOsu(host);

                var temp = prepareTempCopy(osz_path);

                Assert.IsTrue(File.Exists(temp));

                var importer = new BeatmapIPCChannel(client);
                if (!importer.ImportAsync(temp).Wait(10000))
                    Assert.Fail(@"IPC took too long to send");

                ensureLoaded(host);

                Assert.IsFalse(File.Exists(temp));
            }
        }

        [Test]
        public void TestImportWhenFileOpen()
        {
            //unfortunately for the time being we need to reference osu.Framework.Desktop for a game host here.
            using (HeadlessGameHost host = new HeadlessGameHost())
            {
                loadOsu(host);

                var temp = prepareTempCopy(osz_path);

                Assert.IsTrue(File.Exists(temp));

                using (File.OpenRead(temp))
                    host.Dependencies.Get<BeatmapDatabase>().Import(temp);

                ensureLoaded(host);

                Assert.IsTrue(File.Exists(temp));

                File.Delete(temp);

                Assert.IsFalse(File.Exists(temp));
            }
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

            while (!osu.IsLoaded)
                Thread.Sleep(1);

            //reset beatmap database (sqlite and storage backing)
            host.Dependencies.Get<RulesetDatabase>().Reset();
            host.Dependencies.Get<BeatmapDatabase>().Reset();

            return osu;
        }

        private void ensureLoaded(GameHost host, int timeout = 10000)
        {
            IEnumerable<BeatmapSetInfo> resultSets = null;

            Action waitAction = () =>
            {
                while (!(resultSets = host.Dependencies.Get<BeatmapDatabase>()
                    .Query<BeatmapSetInfo>().Where(s => s.OnlineBeatmapSetID == 241526)).Any())
                    Thread.Sleep(50);
            };

            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout),
                @"BeatmapSet did not import to the database in allocated time.");

            //ensure we were stored to beatmap database backing...

            Assert.IsTrue(resultSets.Count() == 1, $@"Incorrect result count found ({resultSets.Count()} but should be 1).");

            IEnumerable<BeatmapInfo> resultBeatmaps = null;

            //if we don't re-check here, the set will be inserted but the beatmaps won't be present yet.
            waitAction = () =>
            {
                while ((resultBeatmaps = host.Dependencies.Get<BeatmapDatabase>()
                    .GetAllWithChildren<BeatmapInfo>(s => s.OnlineBeatmapSetID == 241526 && s.BaseDifficultyID > 0)).Count() != 12)
                    Thread.Sleep(50);
            };

            Assert.IsTrue(waitAction.BeginInvoke(null, null).AsyncWaitHandle.WaitOne(timeout),
                @"Beatmaps did not import to the database in allocated time");

            var set = host.Dependencies.Get<BeatmapDatabase>().GetChildren(resultSets.First());

            Assert.IsTrue(set.Beatmaps.Count == resultBeatmaps.Count(),
                $@"Incorrect database beatmap count post-import ({resultBeatmaps.Count()} but should be {set.Beatmaps.Count}).");

            foreach (BeatmapInfo b in resultBeatmaps)
                Assert.IsTrue(set.Beatmaps.Any(c => c.OnlineBeatmapID == b.OnlineBeatmapID));

            Assert.IsTrue(set.Beatmaps.Count > 0);

            var beatmap = host.Dependencies.Get<BeatmapDatabase>().GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 0))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);

            beatmap = host.Dependencies.Get<BeatmapDatabase>().GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 1))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);

            beatmap = host.Dependencies.Get<BeatmapDatabase>().GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 2))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);

            beatmap = host.Dependencies.Get<BeatmapDatabase>().GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 3))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Count > 0);
        }
    }
}

