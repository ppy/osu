// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mods;
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
                if (hitObject.CreateJudgement().MaxResult.AffectsCombo())
                    combo++;

                foreach (var nested in hitObject.NestedHitObjects)
                    addCombo(nested, ref combo);
            }
        }

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

        /// <summary>
        /// Gets the BPM of a beatmap scaled by active mods of Type T
        /// </summary>
        /// <param name="beatmap">Beatmap, to get base BPM and length from</param>
        /// <param name="mods">Mods to be applied to </param>
        /// <param name="time">The beatmap time at which the mods should be applied</param>
        /// <returns>Adjusted (Minimum BPM, Average BPM, Maximum BPM, Length)</returns>
        public static (int, int, int, double) GetBPMAndLength(this IBeatmap beatmap, IEnumerable<IApplicableToRate> mods, double time = 0)
        {
            // this doesn't consider mods which apply variable rates, yet.
            double rate = 1;

            foreach (var mod in mods)
                rate = mod.ApplyToRate(time, rate);

            int bpmMin = (int)Math.Round(Math.Round(beatmap.ControlPointInfo.BPMMinimum) * rate);
            int bpmAverage = (int)Math.Round(60000 / beatmap.GetMostCommonBeatLength() * rate);
            int bpmMax = (int)Math.Round(Math.Round(beatmap.ControlPointInfo.BPMMaximum) * rate);
            double length = beatmap.BeatmapInfo.Length / rate;

            return (bpmMin, bpmAverage, bpmMax, length);
        }
    }
}
