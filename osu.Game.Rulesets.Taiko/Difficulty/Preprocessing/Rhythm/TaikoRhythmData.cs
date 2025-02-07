// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    /// <summary>
    /// Stores rhythm data for a <see cref="TaikoDifficultyHitObject"/>.
    /// </summary>
    public class TaikoRhythmData
    {
        /// <summary>
        /// The group of hit objects with consistent rhythm that this object belongs to.
        /// </summary>
        public SameRhythmHitObjectGrouping? SameRhythmGroupedHitObjects;

        /// <summary>
        /// The larger pattern of rhythm groups that this object is part of.
        /// </summary>
        public SamePatternsGroupedHitObjects? SamePatternsGroupedHitObjects;

        /// <summary>
        /// The ratio of current <see cref="Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/>
        /// to previous <see cref="Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/> for the rhythm change.
        /// A <see cref="Ratio"/> above 1 indicates a slow-down; a <see cref="Ratio"/> below 1 indicates a speed-up.
        /// </summary>
        /// <remarks>
        /// This is snapped to the closest matching <see cref="common_ratios"/>.
        /// </remarks>
        public readonly double Ratio;

        /// <summary>
        /// Initialises a new instance of <see cref="TaikoRhythmData"/>s,
        /// calculating the closest rhythm change and its associated difficulty for the current hit object.
        /// </summary>
        /// <param name="current">The current <see cref="TaikoDifficultyHitObject"/> being processed.</param>
        public TaikoRhythmData(TaikoDifficultyHitObject current)
        {
            var previous = current.Previous(0);

            if (previous == null)
            {
                Ratio = 1;
                return;
            }

            double actualRatio = current.DeltaTime / previous.DeltaTime;
            double closestRatio = common_ratios.MinBy(r => Math.Abs(r - actualRatio));

            Ratio = closestRatio;
        }

        /// <summary>
        /// List of most common rhythm changes in taiko maps. Based on how each object's interval compares to the previous object.
        /// </summary>
        /// <remarks>
        /// The general guidelines for the values are:
        /// <list type="bullet">
        /// <item>rhythm changes with ratio closer to 1 (that are <i>not</i> 1) are harder to play,</item>
        /// <item>speeding up is <i>generally</i> harder than slowing down (with exceptions of rhythm changes requiring a hand switch).</item>
        /// </list>
        /// </remarks>
        private static readonly double[] common_ratios =
        [
            1.0 / 1,
            2.0 / 1,
            1.0 / 2,
            3.0 / 1,
            1.0 / 3,
            3.0 / 2,
            2.0 / 3,
            5.0 / 4,
            4.0 / 5
        ];
    }
}
