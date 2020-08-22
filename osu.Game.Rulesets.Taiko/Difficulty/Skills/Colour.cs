// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Colour : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        private const int mono_history_max_length = 5;

        /// <summary>
        /// List of the last <see cref="mono_history_max_length"/> most recent mono patterns, with the most recent at the end of the list.
        /// </summary>
        private readonly LimitedCapacityQueue<int> monoHistory = new LimitedCapacityQueue<int>(mono_history_max_length);

        private HitType? previousHitType;

        /// <summary>
        /// Length of the current mono pattern.
        /// </summary>
        private int currentMonoLength = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (!(current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000))
            {
                previousHitType = null;
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
            const int l = 2;
            double penalty = 1.0;

            monoHistory.Enqueue(currentMonoLength);

            for (int start = monoHistory.Count - l - 1; start >= 0; start--)
            {
                if (!isSamePattern(start, l))
                    continue;

                int notesSince = 0;
                for (int i = start; i < monoHistory.Count; i++) notesSince += monoHistory[i];
                penalty *= repetitionPenalty(notesSince);
                break;
            }

            return penalty;
        }

        private bool isSamePattern(int start, int l)
        {
            for (int i = 0; i < l; i++)
            {
                if (monoHistory[start + i] != monoHistory[monoHistory.Count - l + i])
                    return false;
            }

            return true;
        }

        private double repetitionPenalty(int notesSince) => Math.Min(1.0, 0.032 * notesSince);
    }
}
