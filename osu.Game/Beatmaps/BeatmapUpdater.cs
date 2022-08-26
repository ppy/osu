// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles all processing required to ensure a local beatmap is in a consistent state with any changes.
    /// </summary>
    public class BeatmapUpdater : IDisposable
    {
        private readonly IWorkingBeatmapCache workingBeatmapCache;

        private readonly BeatmapDifficultyCache difficultyCache;

        private readonly BeatmapUpdaterMetadataLookup metadataLookup;

        private const int update_queue_request_concurrency = 4;

        private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(update_queue_request_concurrency, nameof(BeatmapUpdaterMetadataLookup));

        public BeatmapUpdater(IWorkingBeatmapCache workingBeatmapCache, BeatmapDifficultyCache difficultyCache, IAPIProvider api, Storage storage)
        {
            this.workingBeatmapCache = workingBeatmapCache;
            this.difficultyCache = difficultyCache;

            metadataLookup = new BeatmapUpdaterMetadataLookup(api, storage);
        }

        /// <summary>
        /// Queue a beatmap for background processing.
        /// </summary>
        /// <param name="beatmapSet">The managed beatmap set to update. A transaction will be opened to apply changes.</param>
        /// <param name="preferOnlineFetch">Whether metadata from an online source should be preferred. If <c>true</c>, the local cache will be skipped to ensure the freshest data state possible.</param>
        public void Queue(Live<BeatmapSetInfo> beatmapSet, bool preferOnlineFetch = false)
        {
            Logger.Log($"Queueing change for local beatmap {beatmapSet}");
            Task.Factory.StartNew(() => beatmapSet.PerformRead(b => Process(b, preferOnlineFetch)), default, TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);
        }

        /// <summary>
        /// Run all processing on a beatmap immediately.
        /// </summary>
        /// <param name="beatmapSet">The managed beatmap set to update. A transaction will be opened to apply changes.</param>
        /// <param name="preferOnlineFetch">Whether metadata from an online source should be preferred. If <c>true</c>, the local cache will be skipped to ensure the freshest data state possible.</param>
        public void Process(BeatmapSetInfo beatmapSet, bool preferOnlineFetch = false) => beatmapSet.Realm.Write(r =>
        {
            // Before we use below, we want to invalidate.
            workingBeatmapCache.Invalidate(beatmapSet);

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            foreach (var beatmap in beatmapSet.Beatmaps)
            {
                difficultyCache.Invalidate(beatmap);

                var working = workingBeatmapCache.GetWorkingBeatmap(beatmap);
                var ruleset = working.BeatmapInfo.Ruleset.CreateInstance();

                Debug.Assert(ruleset != null);

                var calculator = ruleset.CreateDifficultyCalculator(working);

                beatmap.StarRating = calculator.Calculate().StarRating;
                beatmap.Length = calculateLength(working.Beatmap);
                beatmap.BPM = 60000 / working.Beatmap.GetMostCommonBeatLength();
            }

            // And invalidate again afterwards as re-fetching the most up-to-date database metadata will be required.
            workingBeatmapCache.Invalidate(beatmapSet);
        });

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

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (metadataLookup.IsNotNull())
                metadataLookup.Dispose();

            if (updateScheduler.IsNotNull())
                updateScheduler.Dispose();
        }

        #endregion
    }
}
