// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A materialised beatmap.
    /// Generally this interface will be implemented alongside <see cref="IBeatmap{T}"/>, which exposes the ruleset-typed hit objects.
    /// </summary>
    public interface IBeatmap
    {
        /// <summary>
        /// This beatmap's info.
        /// </summary>
        BeatmapInfo BeatmapInfo { get; set; }

        /// <summary>
        /// This beatmap's metadata.
        /// </summary>
        BeatmapMetadata Metadata { get; }

        /// <summary>
        /// This beatmap's difficulty settings.
        /// </summary>
        public BeatmapDifficulty Difficulty { get; set; }

        /// <summary>
        /// The control points in this beatmap.
        /// </summary>
        ControlPointInfo ControlPointInfo { get; set; }

        /// <summary>
        /// The breaks in this beatmap.
        /// </summary>
        List<BreakPeriod> Breaks { get; }

        /// <summary>
        /// Total amount of break time in the beatmap.
        /// </summary>
        double TotalBreakTime { get; }

        /// <summary>
        /// The hitobjects contained by this beatmap.
        /// </summary>
        IReadOnlyList<HitObject> HitObjects { get; }

        /// <summary>
        /// Returns statistics for the <see cref="HitObjects"/> contained in this beatmap.
        /// </summary>
        IEnumerable<BeatmapStatistic> GetStatistics();

        /// <summary>
        /// Finds the most common beat length represented by the control points in this beatmap.
        /// </summary>
        double GetMostCommonBeatLength();

        /// <summary>
        /// Creates a shallow-clone of this beatmap and returns it.
        /// </summary>
        /// <returns>The shallow-cloned beatmap.</returns>
        IBeatmap Clone();
    }

    /// <summary>
    /// A materialised beatmap containing converted HitObjects.
    /// </summary>
    public interface IBeatmap<out T> : IBeatmap
        where T : HitObject
    {
        /// <summary>
        /// The hitobjects contained by this beatmap.
        /// </summary>
        new IReadOnlyList<T> HitObjects { get; }
    }

    public static class BeatmapExtensions
    {
        /// <summary>
        /// Finds the maximum achievable combo by hitting all <see cref="HitObject"/>s in a beatmap.
        /// </summary>
        public static int GetMaxCombo(this IBeatmap beatmap)
        {
            int combo = 0;
            foreach (var h in beatmap.HitObjects)
                addCombo(h, ref combo);
            return combo;

            static void addCombo(HitObject hitObject, ref int combo)
            {
                var judgement = hitObject.CreateJudgement();

                if (judgement.AffectsCombo(judgement.MaxResult) && judgement.MaxResult.IncreasesCombo())
                    combo++;

                foreach (var nested in hitObject.NestedHitObjects)
                    addCombo(nested, ref combo);
            }
        }

        /// <summary>
        /// Find the total milliseconds between the first and last hittable objects.
        /// </summary>
        /// <remarks>
        /// This is cached to <see cref="BeatmapInfo.Length"/>, so using that is preferable when available.
        /// </remarks>
        public static double CalculatePlayableLength(this IBeatmap beatmap) => CalculatePlayableLength(beatmap.HitObjects);

        /// <summary>
        /// Find the total milliseconds between the first and last hittable objects, excluding any break time.
        /// </summary>
        public static double CalculateDrainLength(this IBeatmap beatmap) => CalculatePlayableLength(beatmap.HitObjects) - beatmap.TotalBreakTime;

        /// <summary>
        /// Find the timestamps in milliseconds of the start and end of the playable region.
        /// </summary>
        public static (double start, double end) CalculatePlayableBounds(this IBeatmap beatmap) => CalculatePlayableBounds(beatmap.HitObjects);

        /// <summary>
        /// Find the absolute end time of the latest <see cref="HitObject"/> in a beatmap. Will throw if beatmap contains no objects.
        /// </summary>
        /// <remarks>
        /// This correctly accounts for rulesets which have concurrent hitobjects which may have durations, causing the .Last() object
        /// to not necessarily have the latest end time.
        ///
        /// It's not super efficient so calls should be kept to a minimum.
        /// </remarks>
        public static double GetLastObjectTime(this IBeatmap beatmap) => beatmap.HitObjects.Max(h => h.GetEndTime());

        #region Helper methods

        /// <summary>
        /// Find the total milliseconds between the first and last hittable objects.
        /// </summary>
        /// <remarks>
        /// This is cached to <see cref="BeatmapInfo.Length"/>, so using that is preferable when available.
        /// </remarks>
        public static double CalculatePlayableLength(IEnumerable<HitObject> objects)
        {
            (double start, double end) = CalculatePlayableBounds(objects);

            return end - start;
        }

        /// <summary>
        /// Find the timestamps in milliseconds of the start and end of the playable region.
        /// </summary>
        public static (double start, double end) CalculatePlayableBounds(IEnumerable<HitObject> objects)
        {
            if (!objects.Any())
                return (0, 0);

            double lastObjectTime = objects.Max(o => o.GetEndTime());
            double firstObjectTime = objects.First().StartTime;

            return (firstObjectTime, lastObjectTime);
        }

        #endregion
    }
}
