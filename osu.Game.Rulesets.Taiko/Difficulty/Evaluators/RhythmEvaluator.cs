
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

            difficulty += terms / (1 + ratio);

            // Give bonus to near-1 ratios
            difficulty += DifficultyCalculationUtils.BellCurve(ratio, 1, 0.7);

            // Penalize ratios that are VERY near 1
            difficulty -= DifficultyCalculationUtils.BellCurve(ratio, 1, 0.5);

            difficulty = Math.Max(difficulty, 0);

            return difficulty / Math.Sqrt(8);
        }

        private static double evaluateDifficultyOf(SameRhythmHitObjects sameRhythmHitObjects, double hitWindow)
        {
            double intervalDifficulty = ratioDifficulty(sameRhythmHitObjects.HitObjectIntervalRatio);
            double? previousInterval = sameRhythmHitObjects.Previous?.HitObjectInterval;

            // If a previous interval exists and there are multiple hit objects in the sequence:
            if (previousInterval != null && sameRhythmHitObjects.Children.Count > 1)
            {
                double expectedDurationFromPrevious = (double)previousInterval * sameRhythmHitObjects.Children.Count;
                double durationDifference = sameRhythmHitObjects.Duration - expectedDurationFromPrevious;

                if (durationDifference > 0)
                {
                    intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                        durationDifference / hitWindow,
                        midpointOffset: 0.7,
                        multiplier: 1.5,
                        maxValue: 1);
                }
            }

            // Penalise patterns that can be hit within a single hit window.
            intervalDifficulty *= DifficultyCalculationUtils.Logistic(
                sameRhythmHitObjects.Duration / hitWindow,
                midpointOffset: 0.6,
                multiplier: 1,
                maxValue: 1);

            return Math.Pow(intervalDifficulty, 0.75);
        }

        private static double evaluateDifficultyOf(SamePatterns samePatterns)
        {
            return ratioDifficulty(samePatterns.IntervalRatio);
        }

        /// <summary>
        /// Evaluate the difficulty of a hitobject considering its interval change.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject, double hitWindow)
        {
            TaikoDifficultyHitObjectRhythm rhythm = ((TaikoDifficultyHitObject)hitObject).Rhythm;
            double difficulty = 0.0d;

            if (rhythm.SameRhythmHitObjects?.FirstHitObject == hitObject) // Difficulty for SameRhythmHitObjects
                difficulty += evaluateDifficultyOf(rhythm.SameRhythmHitObjects, hitWindow);

            if (rhythm.SamePatterns?.FirstHitObject == hitObject) // Difficulty for SamePatterns
                difficulty += 1.5 * evaluateDifficultyOf(rhythm.SamePatterns);

            return difficulty;
        }
    }
}
