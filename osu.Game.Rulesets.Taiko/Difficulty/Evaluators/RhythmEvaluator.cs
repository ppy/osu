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
        /// Evaluate the difficulty of a hitobject considering its interval change.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject, double hitWindow)
        {
            TaikoRhythmData rhythmData = ((TaikoDifficultyHitObject)hitObject).RhythmData;
            double difficulty = 0.0d;

            double sameRhythm = 0;
            double samePattern = 0;
            double intervalPenalty = 0;

            if (rhythmData.SameRhythmGroupedHitObjects?.FirstHitObject == hitObject) // Difficulty for SameRhythmGroupedHitObjects
            {
                sameRhythm += 10.0 * evaluateDifficultyOf(rhythmData.SameRhythmGroupedHitObjects, hitWindow);
                intervalPenalty = repeatedIntervalPenalty(rhythmData.SameRhythmGroupedHitObjects, hitWindow);
            }

            if (rhythmData.SamePatternsGroupedHitObjects?.FirstHitObject == hitObject) // Difficulty for SamePatternsGroupedHitObjects
                samePattern += 1.15 * ratioDifficulty(rhythmData.SamePatternsGroupedHitObjects.IntervalRatio);

            difficulty += Math.Max(sameRhythm, samePattern) * intervalPenalty;

            return difficulty;
        }

        private static double evaluateDifficultyOf(SameRhythmHitObjectGrouping sameRhythmGroupedHitObjects, double hitWindow)
        {
            double intervalDifficulty = ratioDifficulty(sameRhythmGroupedHitObjects.HitObjectIntervalRatio);
            double? previousInterval = sameRhythmGroupedHitObjects.Previous?.HitObjectInterval;

            intervalDifficulty *= repeatedIntervalPenalty(sameRhythmGroupedHitObjects, hitWindow);

            // If a previous interval exists and there are multiple hit objects in the sequence:
            if (previousInterval != null && sameRhythmGroupedHitObjects.HitObjects.Count > 1)
            {
                double expectedDurationFromPrevious = (double)previousInterval * sameRhythmGroupedHitObjects.HitObjects.Count;
                double durationDifference = sameRhythmGroupedHitObjects.Duration - expectedDurationFromPrevious;

                if (durationDifference > 0)
                {
                    intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                        durationDifference / hitWindow,
                        midpointOffset: 0.7,
                        multiplier: 1.0,
                        maxValue: 1);
                }
            }

            // Penalise patterns that can be hit within a single hit window.
            intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                sameRhythmGroupedHitObjects.Duration / hitWindow,
                midpointOffset: 0.6,
                multiplier: 1,
                maxValue: 1);

            return Math.Pow(intervalDifficulty, 0.75);
        }

        /// <summary>
        /// Determines if the changes in hit object intervals is consistent based on a given threshold.
        /// </summary>
        private static double repeatedIntervalPenalty(SameRhythmHitObjectGrouping sameRhythmGroupedHitObjects, double hitWindow, double threshold = 0.1)
        {
            double longIntervalPenalty = sameInterval(sameRhythmGroupedHitObjects, 3);

            double shortIntervalPenalty = sameRhythmGroupedHitObjects.HitObjects.Count < 6
                ? sameInterval(sameRhythmGroupedHitObjects, 4)
                : 1.0; // Returns a non-penalty if there are 6 or more notes within an interval.

            // The duration penalty is based on hit object duration relative to hitWindow.
            double durationPenalty = Math.Max(1 - sameRhythmGroupedHitObjects.Duration * 2 / hitWindow, 0.5);

            return Math.Min(longIntervalPenalty, shortIntervalPenalty) * durationPenalty;

            double sameInterval(SameRhythmHitObjectGrouping startObject, int intervalCount)
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
                            return 0.80;
                    }
                }

                return 1.0; // No penalty if all intervals are different.
            }
        }

        /// <summary>
        /// Calculates the difficulty of a given ratio using a combination of periodic penalties and bonuses.
        /// </summary>
        private static double ratioDifficulty(double ratio, int terms = 8)
        {
            double difficulty = 0;

            // Validate the ratio by ensuring it is a normal number in cases where maps breach regular mapping conditions.
            ratio = double.IsNormal(ratio) ? ratio : 0;

            for (int i = 1; i <= terms; ++i)
            {
                difficulty += termPenalty(ratio, i, 4, 1);
            }

            difficulty += terms / (1 + ratio);

            // Give bonus to near-1 ratios
            difficulty += DifficultyCalculationUtils.BellCurve(ratio, 1, 0.5);

            // Penalize ratios that are VERY near 1
            difficulty -= DifficultyCalculationUtils.BellCurve(ratio, 1, 0.3);

            difficulty = Math.Max(difficulty, 0);
            difficulty /= Math.Sqrt(8);

            return difficulty;
        }

        /// <summary>
        /// Multiplier for a given denominator term.
        /// </summary>
        private static double termPenalty(double ratio, int denominator, double power, double multiplier) =>
            -multiplier * Math.Pow(Math.Cos(denominator * Math.PI * ratio), power);
    }
}
