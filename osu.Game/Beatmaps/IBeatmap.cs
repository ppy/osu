// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Lists;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
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
        SortedList<BreakPeriod> Breaks { get; set; }

        /// <summary>
        /// All lines from the [Events] section which aren't handled in the encoding process yet.
        /// These lines should be written out to the beatmap file on save or export.
        /// </summary>
        List<string> UnhandledEventLines { get; }

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

        double AudioLeadIn { get; internal set; }

        float StackLeniency { get; internal set; }

        bool SpecialStyle { get; internal set; }

        bool LetterboxInBreaks { get; internal set; }

        bool WidescreenStoryboard { get; internal set; }

        bool EpilepsyWarning { get; internal set; }

        bool SamplesMatchPlaybackRate { get; internal set; }

        /// <summary>
        /// The ratio of distance travelled per time unit.
        /// Generally used to decouple the spacing between hit objects from the enforced "velocity" of the beatmap (see <see cref="DifficultyControlPoint.SliderVelocity"/>).
        /// </summary>
        /// <remarks>
        /// The most common method of understanding is that at a default value of 1.0, the time-to-distance ratio will match the slider velocity of the beatmap
        /// at the current point in time. Increasing this value will make hit objects more spaced apart when compared to the cursor movement required to track a slider.
        ///
        /// This is only a hint property, used by the editor in <see cref="IDistanceSnapProvider"/> implementations. It does not directly affect the beatmap or gameplay.
        /// </remarks>
        double DistanceSpacing { get; internal set; }

        int GridSize { get; internal set; }

        double TimelineZoom { get; internal set; }

        CountdownType Countdown { get; internal set; }

        /// <summary>
        /// The number of beats to move the countdown backwards (compared to its default location).
        /// </summary>
        int CountdownOffset { get; internal set; }

        int[] Bookmarks { get; internal set; }

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
                if (hitObject.Judgement.MaxResult.AffectsCombo())
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
        public static double CalculateDrainLength(this IBeatmap beatmap) => Math.Max(CalculatePlayableLength(beatmap.HitObjects) - beatmap.TotalBreakTime, 0);

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
        /// <exception cref="InvalidOperationException">If <paramref name="beatmap"/> has no objects.</exception>
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
