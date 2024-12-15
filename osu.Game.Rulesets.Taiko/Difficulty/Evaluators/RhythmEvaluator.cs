// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class RhythmEvaluator
    {
        /// <summary>
        /// Multiplier for a given denominator term.
        /// </summary>
        private static double termPenalty(double ratio, int denominator, double power, double multiplier)
        {
            return -multiplier * Math.Pow(Math.Cos(denominator * Math.PI * ratio), power);
        }

        /// <summary>
        /// Calculates the difficulty of a given ratio using a combination of periodic penalties and bonuses.
        /// </summary>
        private static double ratioDifficulty(double ratio, int terms = 8)
        {
            // Sum of n = 8 terms of periodic penalty.
            double difficulty = 0;

            for (int i = 1; i <= terms; ++i)
            {
                difficulty += termPenalty(ratio, i, 2, 1);
            }

            difficulty += terms;

            // Give bonus to near-1 ratios
            difficulty += DifficultyCalculationUtils.BellCurve(ratio, 1, 0.5);

            // Penalize ratios that are VERY near 1
            difficulty -= DifficultyCalculationUtils.BellCurve(ratio, 1, 0.3);

            return difficulty / Math.Sqrt(8);
        }

        /// <summary>
        /// Determines if the pattern of hit object intervals is consistent based on a given threshold.
        /// </summary>
        private static double repeatedIntervalPenalty(EvenHitObjects evenHitObjects, double threshold = 0.1)
        {
            // Collect the last 3 intervals (current and the last 2 previous).
            List<double?> intervals = new List<double?>();
            var currentObject = evenHitObjects;
            const int interval_count = 3;

            for (int i = 0; i < interval_count && currentObject != null; i++)
            {
                intervals.Add(currentObject.HitObjectInterval);
                currentObject = currentObject.Previous;
            }

            intervals.RemoveAll(interval => interval == null);

            // If there are fewer than 3 valid intervals, skip the consistency check.
            if (intervals.Count < interval_count)
                return 1.0; // No penalty applied if there isn't enough data.

            for (int i = 0; i < intervals.Count; i++)
            {
                for (int j = i + 1; j < intervals.Count; j++)
                {
                    double ratio = intervals[i]!.Value / intervals[j]!.Value;
                    if (Math.Abs(1 - ratio) <= threshold) // If any two intervals are similar, apply penalty.
                        return 0.3;
                }
            }

            // No similar intervals were found.
            return 1.0;
        }

        private static double evaluateDifficultyOf(EvenHitObjects evenHitObjects, double hitWindow)
        {
            double intervalDifficulty = ratioDifficulty(evenHitObjects.HitObjectIntervalRatio);
            double? previousInterval = evenHitObjects.Previous?.HitObjectInterval;

            // If a previous interval exists and there are multiple hit objects in the sequence:
            if (previousInterval != null && evenHitObjects.Children.Count > 1)
            {
                double expectedDurationFromPrevious = (double)previousInterval * evenHitObjects.Children.Count;
                double durationDifference = Math.Abs(evenHitObjects.Duration - expectedDurationFromPrevious);

                intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                    durationDifference / hitWindow,
                    midpointOffset: 0.5,
                    multiplier: 1.5,
                    maxValue: 1);
            }

            // Apply consistency penalty
            intervalDifficulty *= repeatedIntervalPenalty(evenHitObjects);

            // Penalise patterns that can be hit within a single hit window.
            intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                evenHitObjects.Duration / hitWindow,
                midpointOffset: 0.5,
                multiplier: 1,
                maxValue: 1);

            return intervalDifficulty;
        }

        private static double evaluateDifficultyOf(EvenPatterns evenPatterns)
        {
            return ratioDifficulty(evenPatterns.IntervalRatio);
        }

        /// <summary>
        /// Evaluate the difficulty of a hitobject considering its interval change.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject, double hitWindow)
        {
            TaikoDifficultyHitObjectRhythm rhythm = ((TaikoDifficultyHitObject)hitObject).Rhythm;
            double difficulty = 0.0d;

            if (rhythm.EvenHitObjects?.FirstHitObject == hitObject) // Difficulty for EvenHitObjects
                difficulty += evaluateDifficultyOf(rhythm.EvenHitObjects, hitWindow);

            if (rhythm.EvenPatterns?.FirstHitObject == hitObject) // Difficulty for EvenPatterns
                difficulty += evaluateDifficultyOf(rhythm.EvenPatterns) * rhythm.Difficulty;

            return difficulty;
        }
    }
}
