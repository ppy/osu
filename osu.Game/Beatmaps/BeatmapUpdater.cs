// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Game.Database;
using osu.Game.Rulesets.Objects;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles all processing required to ensure a local beatmap is in a consistent state with any changes.
    /// </summary>
    public class BeatmapUpdater
    {
        private readonly IWorkingBeatmapCache workingBeatmapCache;
        private readonly BeatmapOnlineLookupQueue onlineLookupQueue;
        private readonly BeatmapDifficultyCache difficultyCache;

        public BeatmapUpdater(IWorkingBeatmapCache workingBeatmapCache, BeatmapOnlineLookupQueue onlineLookupQueue, BeatmapDifficultyCache difficultyCache)
        {
            this.workingBeatmapCache = workingBeatmapCache;
            this.onlineLookupQueue = onlineLookupQueue;
            this.difficultyCache = difficultyCache;
        }

        /// <summary>
        /// Queue a beatmap for background processing.
        /// </summary>
        public void Queue(Live<BeatmapSetInfo> beatmap)
        {
            // For now, just fire off a task.
            // TODO: Add actual queueing probably.
            Task.Factory.StartNew(() => beatmap.PerformRead(Process));
        }

        /// <summary>
        /// Run all processing on a beatmap immediately.
        /// </summary>
        public void Process(BeatmapSetInfo beatmapSet) => beatmapSet.Realm.Write(r => Process(beatmapSet, r));

        public void Process(BeatmapSetInfo beatmapSet, Realm realm)
        {
            // Before we use below, we want to invalidate.
            workingBeatmapCache.Invalidate(beatmapSet);

            onlineLookupQueue.Update(beatmapSet);

            foreach (var beatmap in beatmapSet.Beatmaps)
            {
                // Because we aren't guaranteed all processing will happen on this thread, it's very hard to use the live realm object.
                // This can be fixed by adding a synchronous flow to `BeatmapDifficultyCache`.
                var detachedBeatmap = beatmap.Detach();

                beatmap.StarRating = difficultyCache.GetDifficultyAsync(detachedBeatmap).GetResultSafely()?.Stars ?? 0;

                var working = workingBeatmapCache.GetWorkingBeatmap(beatmap);
                beatmap.Length = calculateLength(working.Beatmap);
                beatmap.BPM = 60000 / working.Beatmap.GetMostCommonBeatLength();
            }
        }

        private double calculateLength(IBeatmap b)
        {
            if (!b.HitObjects.Any())
                return 0;

            var lastObject = b.HitObjects.Last();

            //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
            double endTime = lastObject.GetEndTime();
            double startTime = b.HitObjects.First().StartTime;

            return endTime - startTime;
        }
    }
}
