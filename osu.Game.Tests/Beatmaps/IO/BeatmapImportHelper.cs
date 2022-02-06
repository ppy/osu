// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Tests.Database;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.IO
{
    public static class BeatmapImportHelper
    {
        public static async Task<BeatmapSetInfo> LoadQuickOszIntoOsu(OsuGameBase osu)
        {
            string temp = TestResources.GetQuickTestBeatmapForImport();

            var manager = osu.Dependencies.Get<BeatmapManager>();

            var importedSet = await manager.Import(new ImportTask(temp)).ConfigureAwait(false);

            Debug.Assert(importedSet != null);

            ensureLoaded(osu);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return manager.GetAllUsableBeatmapSets().Find(beatmapSet => beatmapSet.ID == importedSet.ID);
        }

        public static async Task<BeatmapSetInfo> LoadOszIntoOsu(OsuGameBase osu, string path = null, bool virtualTrack = false)
        {
            string temp = path ?? TestResources.GetTestBeatmapForImport(virtualTrack);

            var manager = osu.Dependencies.Get<BeatmapManager>();

            var importedSet = await manager.Import(new ImportTask(temp)).ConfigureAwait(false);

            Debug.Assert(importedSet != null);

            ensureLoaded(osu);

            waitForOrAssert(() => !File.Exists(temp), "Temporary file still exists after standard import", 5000);

            return manager.GetAllUsableBeatmapSets().Find(beatmapSet => beatmapSet.ID == importedSet.ID);
        }

        private static void ensureLoaded(OsuGameBase osu, int timeout = 60000)
        {
            var realm = osu.Dependencies.Get<RealmAccess>();

            realm.Run(r => BeatmapImporterTests.EnsureLoaded(r, timeout));

            // TODO: add back some extra checks outside of the realm ones?
            // var set = queryBeatmapSets().First();
            // foreach (BeatmapInfo b in set.Beatmaps)
            //     Assert.IsTrue(set.Beatmaps.Any(c => c.OnlineID == b.OnlineID));
            // Assert.IsTrue(set.Beatmaps.Count > 0);
            // var beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 0))?.Beatmap;
            // Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            // beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 1))?.Beatmap;
            // Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            // beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 2))?.Beatmap;
            // Assert.IsTrue(beatmap?.HitObjects.Any() == true);
            // beatmap = store.GetWorkingBeatmap(set.Beatmaps.First(b => b.RulesetID == 3))?.Beatmap;
            // Assert.IsTrue(beatmap?.HitObjects.Any() == true);
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
