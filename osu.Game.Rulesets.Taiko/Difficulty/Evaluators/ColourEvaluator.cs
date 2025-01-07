// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class ColourEvaluator
    {
        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="MonoStreak"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(MonoStreak monoStreak)
        {
            return DifficultyCalculationUtils.Logistic(exponent: Math.E * monoStreak.Index - 2 * Math.E) * EvaluateDifficultyOf(monoStreak.Parent) * 0.5;
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="AlternatingMonoPattern"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(AlternatingMonoPattern alternatingMonoPattern)
        {
            return DifficultyCalculationUtils.Logistic(exponent: Math.E * alternatingMonoPattern.Index - 2 * Math.E) * EvaluateDifficultyOf(alternatingMonoPattern.Parent);
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="RepeatingHitPatterns"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(RepeatingHitPatterns repeatingHitPattern)
        {
            return 2 * (1 - DifficultyCalculationUtils.Logistic(exponent: Math.E * repeatingHitPattern.RepetitionInterval - 2 * Math.E));
        }

        /// <summary>
        /// Calculates a consistency penalty based on the number of consecutive consistent intervals,
        /// considering the delta time between each colour sequence.
        /// </summary>
        /// <param name="hitObject">The current hitObject to consider.</param>
        /// <param name="threshold"> The allowable margin of error for determining whether ratios are consistent.</param>
        /// <param name="maxObjectsToCheck">The maximum objects to check per count of consistent ratio.</param>
        private static double consistentRatioPenalty(TaikoDifficultyHitObject hitObject, double threshold = 0.01, int maxObjectsToCheck = 64)
        {
            int consistentRatioCount = 0;
            double totalRatioCount = 0.0;

            TaikoDifficultyHitObject current = hitObject;

            for (int i = 0; i < maxObjectsToCheck; i++)
            {
                // Break if there is no valid previous object
                if (current.Index <= 1)
                    break;

                var previousHitObject = (TaikoDifficultyHitObject)current.Previous(1);

                double currentRatio = current.Rhythm.Ratio;
                double previousRatio = previousHitObject.Rhythm.Ratio;

                // A consistent interval is defined as the percentage difference between the two rhythmic ratios with the margin of error.
                if (Math.Abs(1 - currentRatio / previousRatio) <= threshold)
                {
                    consistentRatioCount++;
                    totalRatioCount += currentRatio;
                    break;
                }

                // Move to the previous object
                current = previousHitObject;
            }

            // Ensure no division by zero
            double ratioPenalty = 1 - totalRatioCount / (consistentRatioCount + 1) * 0.80;

            return ratioPenalty;
        }

        /// <summary>
        /// Evaluate the difficulty of the first hitobject within a colour streak.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject)
        {
            var taikoObject = (TaikoDifficultyHitObject)hitObject;
            TaikoDifficultyHitObjectColour colour = taikoObject.Colour;
            double difficulty = 0.0d;

            if (colour.MonoStreak?.FirstHitObject == hitObject) // Difficulty for MonoStreak
                difficulty += EvaluateDifficultyOf(colour.MonoStreak);

            if (colour.AlternatingMonoPattern?.FirstHitObject == hitObject) // Difficulty for AlternatingMonoPattern
                difficulty += EvaluateDifficultyOf(colour.AlternatingMonoPattern);

            if (colour.RepeatingHitPattern?.FirstHitObject == hitObject) // Difficulty for RepeatingHitPattern
                difficulty += EvaluateDifficultyOf(colour.RepeatingHitPattern);

            double consistencyPenalty = consistentRatioPenalty(taikoObject);
            difficulty *= consistencyPenalty;

            return difficulty;
        }
    }
}
