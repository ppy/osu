// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the colour coefficient of taiko difficulty.
    /// </summary>
    public class Colour : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        /// <summary>
        /// Maximum number of entries to keep in <see cref="monoHistory"/>.
        /// </summary>
        private const int mono_history_max_length = 5;

        /// <summary>
        /// Queue with the lengths of the last <see cref="mono_history_max_length"/> most recent mono (single-colour) patterns,
        /// with the most recent value at the end of the queue.
        /// </summary>
        private readonly LimitedCapacityQueue<int> monoHistory = new LimitedCapacityQueue<int>(mono_history_max_length);

        /// <summary>
        /// The <see cref="HitType"/> of the last object hit before the one being considered.
        /// </summary>
        private HitType? previousHitType;

        /// <summary>
        /// Length of the current mono pattern.
        /// </summary>
        private int currentMonoLength;

        public Colour(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // changing from/to a drum roll or a swell does not constitute a colour change.
            // hits spaced more than a second apart are also exempt from colour strain.
            if (!(current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000))
            {
                monoHistory.Clear();

                var currentHit = current.BaseObject as Hit;
                currentMonoLength = currentHit != null ? 1 : 0;
                previousHitType = currentHit?.Type;

                return 0.0;
            }

            var taikoCurrent = (TaikoDifficultyHitObject)current;

            double objectStrain = 0.0;

            if (previousHitType != null && taikoCurrent.HitType != previousHitType)
            {
                // The colour has changed.
                objectStrain = 1.0;

                if (monoHistory.Count < 2)
                {
                    // There needs to be at least two streaks to determine a strain.
                    objectStrain = 0.0;
                }
                else if ((monoHistory[^1] + currentMonoLength) % 2 == 0)
                {
                    // The last streak in the history is guaranteed to be a different type to the current streak.
                    // If the total number of notes in the two streaks is even, nullify this object's strain.
                    objectStrain = 0.0;
                }

                objectStrain *= repetitionPenalties();
                currentMonoLength = 1;
            }
            else
            {
                currentMonoLength += 1;
            }

            previousHitType = taikoCurrent.HitType;
            return objectStrain;
        }

        /// <summary>
        /// The penalty to apply due to the length of repetition in colour streaks.
        /// </summary>
        private double repetitionPenalties()
        {
            const int most_recent_patterns_to_compare = 2;
            double penalty = 1.0;

            monoHistory.Enqueue(currentMonoLength);

            for (int start = monoHistory.Count - most_recent_patterns_to_compare - 1; start >= 0; start--)
            {
                if (!isSamePattern(start, most_recent_patterns_to_compare))
                    continue;

                int notesSince = 0;
                for (int i = start; i < monoHistory.Count; i++) notesSince += monoHistory[i];
                penalty *= repetitionPenalty(notesSince);
                break;
            }

            return penalty;
        }

        /// <summary>
        /// Determines whether the last <paramref name="mostRecentPatternsToCompare"/> patterns have repeated in the history
        /// of single-colour note sequences, starting from <paramref name="start"/>.
        /// </summary>
        private bool isSamePattern(int start, int mostRecentPatternsToCompare)
        {
            for (int i = 0; i < mostRecentPatternsToCompare; i++)
            {
                if (monoHistory[start + i] != monoHistory[monoHistory.Count - mostRecentPatternsToCompare + i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the strain penalty for a colour pattern repetition.
        /// </summary>
        /// <param name="notesSince">The number of notes since the last repetition of the pattern.</param>
        private double repetitionPenalty(int notesSince) => Math.Min(1.0, 0.032 * notesSince);
    }
}
