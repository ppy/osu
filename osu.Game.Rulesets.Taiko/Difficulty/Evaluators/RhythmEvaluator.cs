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
            double difficulty = 0;

            for (int i = 1; i <= terms; ++i)
            {
                difficulty += termPenalty(ratio, i, 2, 1);
            }

            difficulty += terms;

            // Give bonus to near-1 ratios
            difficulty += DifficultyCalculationUtils.BellCurve(ratio, 1, 0.7);

            // Penalize ratios that are VERY near 1
            difficulty -= DifficultyCalculationUtils.BellCurve(ratio, 1, 0.5);

            return difficulty / Math.Sqrt(8);
        }

        /// <summary>
        /// Determines if the changes in hit object intervals is consistent based on a given threshold.
        /// </summary>
        private static double repeatedIntervalPenalty(EvenHitObjects evenHitObjects, double hitWindow, double threshold = 0.1)
        {
            double longIntervalPenalty = sameInterval(evenHitObjects, 3);

            double shortIntervalPenalty = evenHitObjects.Children.Count < 6
                ? sameInterval(evenHitObjects, 4)
                : 1.0; // Returns a non-penalty if there are 6 or more notes within an interval.

            // Scale penalties dynamically based on hit object duration relative to hitWindow.
            double penaltyScaling = Math.Max(1 - evenHitObjects.Duration / (hitWindow * 2), 0.5);

            return Math.Min(longIntervalPenalty, shortIntervalPenalty) * penaltyScaling;

            double sameInterval(EvenHitObjects startObject, int intervalCount)
            {
                List<double?> intervals = new List<double?>();
                var currentObject = startObject;

                for (int i = 0; i < intervalCount && currentObject != null; i++)
                {
                    intervals.Add(currentObject.HitObjectInterval);
                    currentObject = currentObject.Previous;
                }

                intervals.RemoveAll(interval => interval == null);

                if (intervals.Count < intervalCount)
                    return 1.0; // No penalty if there aren't enough valid intervals.

                for (int i = 0; i < intervals.Count; i++)
                {
                    for (int j = i + 1; j < intervals.Count; j++)
                    {
                        double ratio = intervals[i]!.Value / intervals[j]!.Value;
                        if (Math.Abs(1 - ratio) <= threshold) // If any two intervals are similar, apply a penalty.
                            return 0.3;
                    }
                }

                return 1.0; // No penalty if all intervals are different.
            }
        }

        private static double evaluateDifficultyOf(EvenHitObjects evenHitObjects, double hitWindow)
        {
            double intervalDifficulty = ratioDifficulty(evenHitObjects.HitObjectIntervalRatio);
            double? previousInterval = evenHitObjects.Previous?.HitObjectInterval;

            // If a previous interval exists and there are multiple hit objects in the sequence:
            if (previousInterval != null && evenHitObjects.Children.Count > 1)
            {
                double expectedDurationFromPrevious = (double)previousInterval * evenHitObjects.Children.Count;
                double durationDifference = evenHitObjects.Duration - expectedDurationFromPrevious;

                if (durationDifference > 0)
                {
                    intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                        durationDifference / hitWindow,
                        midpointOffset: 0.7,
                        multiplier: 1.5,
                        maxValue: 1);
                }
            }

            // Apply consistency penalty.
            intervalDifficulty *= repeatedIntervalPenalty(evenHitObjects, hitWindow);

            // Penalise patterns that can be hit within a single hit window.
            intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                evenHitObjects.Duration / hitWindow,
                midpointOffset: 0.6,
                multiplier: 1,
                maxValue: 1);

            return Math.Pow(intervalDifficulty, 0.75);
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
                difficulty += 0.5 * evaluateDifficultyOf(rhythm.EvenPatterns);

            return difficulty;
        }
    }
}
