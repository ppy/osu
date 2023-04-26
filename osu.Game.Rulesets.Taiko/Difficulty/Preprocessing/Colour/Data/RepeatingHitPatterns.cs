// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data
{
    /// <summary>
    /// Encodes a list of <see cref="AlternatingMonoPattern"/>s, grouped together by back and forth repetition of the same
    /// <see cref="AlternatingMonoPattern"/>. Also stores the repetition interval between this and the previous <see cref="RepeatingHitPatterns"/>.
    /// </summary>
    public class RepeatingHitPatterns
    {
        /// <summary>
        /// Maximum amount of <see cref="RepeatingHitPatterns"/>s to look back to find a repetition.
        /// </summary>
        private const int max_repetition_interval = 16;

        /// <summary>
        /// The <see cref="AlternatingMonoPattern"/>s that are grouped together within this <see cref="RepeatingHitPatterns"/>.
        /// </summary>
        public readonly List<AlternatingMonoPattern> AlternatingMonoPatterns = new List<AlternatingMonoPattern>();

        /// <summary>
        /// The first <see cref="TaikoDifficultyHitObject"/> in this <see cref="RepeatingHitPatterns"/>
        /// </summary>
        public TaikoDifficultyHitObject FirstHitObject => AlternatingMonoPatterns[0].FirstHitObject;

        /// <summary>
        /// The previous <see cref="RepeatingHitPatterns"/>. This is used to determine the repetition interval.
        /// </summary>
        public readonly RepeatingHitPatterns? Previous;

        /// <summary>
        /// How many <see cref="RepeatingHitPatterns"/> between the current and previous identical <see cref="RepeatingHitPatterns"/>.
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; } = max_repetition_interval + 1;

        public RepeatingHitPatterns(RepeatingHitPatterns? previous)
        {
            Previous = previous;
        }

        /// <summary>
        /// Returns true if other is considered a repetition of this pattern. This is true if other's first two payloads
        /// have identical mono lengths.
        /// </summary>
        private bool isRepetitionOf(RepeatingHitPatterns other)
        {
            if (AlternatingMonoPatterns.Count != other.AlternatingMonoPatterns.Count) return false;

            for (int i = 0; i < Math.Min(AlternatingMonoPatterns.Count, 2); i++)
            {
                if (!AlternatingMonoPatterns[i].HasIdenticalMonoLength(other.AlternatingMonoPatterns[i])) return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the closest previous <see cref="RepeatingHitPatterns"/> that has the identical <see cref="AlternatingMonoPatterns"/>.
        /// Interval is defined as the amount of <see cref="RepeatingHitPatterns"/> chunks between the current and repeated patterns.
        /// </summary>
        public void FindRepetitionInterval()
        {
            if (Previous == null)
            {
                RepetitionInterval = max_repetition_interval + 1;
                return;
            }

            RepeatingHitPatterns? other = Previous;
            int interval = 1;

            while (interval < max_repetition_interval)
            {
                if (isRepetitionOf(other))
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    return;
                }

                other = other.Previous;
                if (other == null) break;

                ++interval;
            }

            RepetitionInterval = max_repetition_interval + 1;
        }
    }
}
