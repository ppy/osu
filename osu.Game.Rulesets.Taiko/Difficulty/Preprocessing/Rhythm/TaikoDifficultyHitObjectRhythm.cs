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
    public class TaikoDifficultyHitObjectRhythm
    {
        /// <summary>
        /// The group of hit objects with consistent rhythm that this object belongs to.
        /// </summary>
        public SameRhythmHitObjects? SameRhythmHitObjects;

        /// <summary>
        /// The larger pattern of rhythm groups that this object is part of.
        /// </summary>
        public SamePatterns? SamePatterns;

        /// <summary>
        /// The ratio of current <see cref="Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/>
        /// to previous <see cref="Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/> for the rhythm change.
        /// A <see cref="Ratio"/> above 1 indicates a slow-down; a <see cref="Ratio"/> below 1 indicates a speed-up.
        /// </summary>
        public readonly double Ratio;

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
        private static readonly TaikoDifficultyHitObjectRhythm[] common_rhythms =
        {
            new TaikoDifficultyHitObjectRhythm(1, 1),
            new TaikoDifficultyHitObjectRhythm(2, 1),
            new TaikoDifficultyHitObjectRhythm(1, 2),
            new TaikoDifficultyHitObjectRhythm(3, 1),
            new TaikoDifficultyHitObjectRhythm(1, 3),
            new TaikoDifficultyHitObjectRhythm(3, 2),
            new TaikoDifficultyHitObjectRhythm(2, 3),
            new TaikoDifficultyHitObjectRhythm(5, 4),
            new TaikoDifficultyHitObjectRhythm(4, 5)
        };

        /// <summary>
        /// Initialises a new instance of <see cref="TaikoDifficultyHitObjectRhythm"/>s,
        /// calculating the closest rhythm change and its associated difficulty for the current hit object.
        /// </summary>
        /// <param name="current">The current <see cref="TaikoDifficultyHitObject"/> being processed.</param>
        public TaikoDifficultyHitObjectRhythm(TaikoDifficultyHitObject current)
        {
            var previous = current.Previous(0);

            if (previous == null)
            {
                Ratio = 1;
                return;
            }

            TaikoDifficultyHitObjectRhythm closestRhythm = getClosestRhythm(current.DeltaTime, previous.DeltaTime);
            Ratio = closestRhythm.Ratio;
        }

        /// <summary>
        /// Creates an object representing a rhythm change.
        /// </summary>
        /// <param name="numerator">The numerator for <see cref="Ratio"/>.</param>
        /// <param name="denominator">The denominator for <see cref="Ratio"/></param>
        private TaikoDifficultyHitObjectRhythm(int numerator, int denominator)
        {
            Ratio = numerator / (double)denominator;
        }

        /// <summary>
        /// Determines the closest rhythm change from <see cref="common_rhythms"/> that matches the timing ratio
        /// between the current and previous intervals.
        /// </summary>
        /// <param name="currentDeltaTime">The time difference between the current hit object and the previous one.</param>
        /// <param name="previousDeltaTime">The time difference between the previous hit object and the one before it.</param>
        /// <returns>The closest matching rhythm from <see cref="common_rhythms"/>.</returns>
        private TaikoDifficultyHitObjectRhythm getClosestRhythm(double currentDeltaTime, double previousDeltaTime)
        {
            double ratio = currentDeltaTime / previousDeltaTime;
            return common_rhythms.OrderBy(x => Math.Abs(x.Ratio - ratio)).First();
        }
    }
}

