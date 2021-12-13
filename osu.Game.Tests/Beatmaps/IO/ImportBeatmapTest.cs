// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Scoring;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Scores.IO;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class ImportBeatmapTest : ImportTest
    {
        public static async Task<BeatmapSetInfo> LoadQuickOszIntoOsu(OsuGameBase osu)
        {
            string temp = TestResources.GetQuickTestBeatmapForImport();

            var manager = osu.Dependencies.Get<BeatmapManager>();

            var importedSet = manager.Import(new ImportTask(temp)).GetResultSafely();

            ensureLoaded(osu).WaitSafely();

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return manager.GetAllUsableBeatmapSets().Find(beatmapSet => beatmapSet.ID == importedSet.Value.ID);
        }, TaskCreationOptions.LongRunning);

        public static Task<BeatmapSetInfo> LoadOszIntoOsu(OsuGameBase osu, string path = null, bool virtualTrack = false) => Task.Factory.StartNew(() =>
        {
            string temp = path ?? TestResources.GetTestBeatmapForImport(virtualTrack);

            var manager = osu.Dependencies.Get<BeatmapManager>();

            var importedSet = manager.Import(new ImportTask(temp)).GetResultSafely();

            ensureLoaded(osu).WaitSafely();

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return manager.GetAllUsableBeatmapSets().Find(beatmapSet => beatmapSet.ID == importedSet.Value.ID);
        }, TaskCreationOptions.LongRunning);

        private void deleteBeatmapSet(BeatmapSetInfo imported, OsuGameBase osu)
        {
            var manager = osu.Dependencies.Get<BeatmapManager>();
            manager.Delete(imported);

            checkBeatmapSetCount(osu, 0);
            checkBeatmapSetCount(osu, 1, true);
            checkSingleReferencedFileCount(osu, 0);

            Assert.IsTrue(manager.QueryBeatmapSets(_ => true).First().DeletePending);
        }

        private static Task createScoreForBeatmap(OsuGameBase osu, BeatmapInfo beatmapInfo)
        {
            return ImportScoreTest.LoadScoreIntoOsu(osu, new ScoreInfo
            {
                OnlineID = 2,
                BeatmapInfo = beatmapInfo,
            }, new ImportScoreTest.TestArchiveReader());
        }

        private static void checkBeatmapSetCount(OsuGameBase osu, int expected, bool includeDeletePending = false)
        {
            var manager = osu.Dependencies.Get<BeatmapManager>();

            Assert.AreEqual(expected, includeDeletePending
                ? manager.QueryBeatmapSets(_ => true).ToList().Count
                : manager.GetAllUsableBeatmapSets().Count);
        }

        private static string hashFile(string filename)
        {
            using (var s = File.OpenRead(filename))
                return s.ComputeMD5Hash();
        }

        private static void checkBeatmapCount(OsuGameBase osu, int expected)
        {
            Assert.AreEqual(expected, osu.Dependencies.Get<BeatmapManager>().QueryBeatmaps(_ => true).ToList().Count);
        }

        private static void checkSingleReferencedFileCount(OsuGameBase osu, int expected)
        {
            Assert.AreEqual(expected, osu.Dependencies.Get<DatabaseContextFactory>().Get().FileInfo.Count(f => f.ReferenceCount == 1));
        }

        private static Task ensureLoaded(OsuGameBase osu, int timeout = 60000) => Task.Factory.StartNew(() =>
        {
            IEnumerable<BeatmapSetInfo> resultSets = null;
            var store = osu.Dependencies.Get<BeatmapManager>();
            waitForOrAssert(() => (resultSets = store.QueryBeatmapSets(s => s.OnlineID == 241526)).Any(),
                @"BeatmapSet did not import to the database in allocated time.", timeout);

            // ensure we were stored to beatmap database backing...
            Assert.IsTrue(resultSets.Count() == 1, $@"Incorrect result count found ({resultSets.Count()} but should be 1).");
            IEnumerable<BeatmapInfo> queryBeatmaps() => store.QueryBeatmaps(s => s.BeatmapSet.OnlineID == 241526);
            IEnumerable<BeatmapSetInfo> queryBeatmapSets() => store.QueryBeatmapSets(s => s.OnlineID == 241526);

            // if we don't re-check here, the set will be inserted but the beatmaps won't be present yet.
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
                Assert.IsTrue(set.Beatmaps.Any(c => c.OnlineID == b.OnlineID));
            Assert.IsTrue(set.Beatmaps.Count > 0);
            var beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 0))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 1))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 2))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 3))?.Beatmap;
            Assert.IsTrue(beatmap?.HitObjects.Any() == true);
        }, TaskCreationOptions.LongRunning);

        private static void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                while (!result()) Thread.Sleep(200);
            }, TaskCreationOptions.LongRunning);

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
    }
}
