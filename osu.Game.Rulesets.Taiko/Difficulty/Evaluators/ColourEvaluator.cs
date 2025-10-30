// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public static class ColourEvaluator
    {
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

            List<double> recentRatios = new List<double>();
            TaikoDifficultyHitObject current = hitObject;
            var previousHitObject = (TaikoDifficultyHitObject)current.Previous(1);

            for (int i = 0; i < maxObjectsToCheck; i++)
            {
                // Break if there is no valid previous object
                if (current.Index <= 1)
                    break;

                double currentRatio = current.RhythmData.Ratio;
                double previousRatio = previousHitObject.RhythmData.Ratio;

                recentRatios.Add(currentRatio);

                // A consistent interval is defined as the percentage difference between the two rhythmic ratios with the margin of error.
                if (Math.Abs(1 - currentRatio / previousRatio) <= threshold)
                {
                    consistentRatioCount++;
                    totalRatioCount += currentRatio;
                    break;
                }

                current = previousHitObject;
            }

            // Ensure no division by zero
            if (consistentRatioCount > 0)
                return 1 - totalRatioCount / (consistentRatioCount + 1) * 0.80;

            if (recentRatios.Count <= 1) return 1.0;

            // As a fallback, calculate the maximum deviation from the average of the recent ratios to ensure slightly off-snapped objects don't bypass the penalty.
            double maxRatioDeviation = recentRatios.Max(r => Math.Abs(r - recentRatios.Average()));

            double consistentRatioPenalty = 0.7 + 0.3 * DifficultyCalculationUtils.Smootherstep(maxRatioDeviation, 0.0, 1.0);

            return consistentRatioPenalty;
        }

        /// <summary>
        /// Evaluate the difficulty of the first hitobject within a colour streak.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject)
        {
            var taikoObject = (TaikoDifficultyHitObject)hitObject;
            TaikoColourData colourData = taikoObject.ColourData;
            double difficulty = 0.0d;

            if (colourData.MonoStreak?.FirstHitObject == hitObject) // Difficulty for MonoStreak
                difficulty += evaluateMonoStreakDifficulty(colourData.MonoStreak);

            if (colourData.AlternatingMonoPattern?.FirstHitObject == hitObject) // Difficulty for AlternatingMonoPattern
                difficulty += evaluateAlternatingMonoPatternDifficulty(colourData.AlternatingMonoPattern);

            if (colourData.RepeatingHitPattern?.FirstHitObject == hitObject) // Difficulty for RepeatingHitPattern
                difficulty += evaluateRepeatingHitPatternsDifficulty(colourData.RepeatingHitPattern);

            double consistencyPenalty = consistentRatioPenalty(taikoObject);
            difficulty *= consistencyPenalty;

            return difficulty;
        }

        private static double evaluateMonoStreakDifficulty(MonoStreak monoStreak) =>
            DifficultyCalculationUtils.Logistic(exponent: Math.E * monoStreak.Index - 2 * Math.E) * evaluateAlternatingMonoPatternDifficulty(monoStreak.Parent) * 0.5;

        private static double evaluateAlternatingMonoPatternDifficulty(AlternatingMonoPattern alternatingMonoPattern) =>
            DifficultyCalculationUtils.Logistic(exponent: Math.E * alternatingMonoPattern.Index - 2 * Math.E) * evaluateRepeatingHitPatternsDifficulty(alternatingMonoPattern.Parent);

        private static double evaluateRepeatingHitPatternsDifficulty(RepeatingHitPatterns repeatingHitPattern) =>
            2 * (1 - DifficultyCalculationUtils.Logistic(exponent: Math.E * repeatingHitPattern.RepetitionInterval - 2 * Math.E));
    }
}
