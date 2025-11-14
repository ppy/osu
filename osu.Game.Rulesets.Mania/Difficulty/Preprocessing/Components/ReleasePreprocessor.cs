// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Components
{
    public class ReleasePreprocessor
    {
        private const int smoothing_window_ms = 500;

        /// <summary>
        /// Computes the release factor values across all time points.
        /// This focuses on the difficulty of timing long note releases correctly.
        /// </summary>
        public static double[] ComputeValues(ManiaDifficultyContext data)
        {
            double[] baseRelease = calculateBaseReleaseFactor(data);

            // Apply smoothing to create stable difficulty curves
            double[] smoothed = StrainArrayUtils.ApplySmoothingToArray(
                data.CornerData.BaseTimeCorners,
                baseRelease,
                smoothing_window_ms,
                0.001,
                "sum"
            );

            // Interpolate to match target time resolution
            return StrainArrayUtils.InterpolateArray(
                data.CornerData.TimeCorners,
                data.CornerData.BaseTimeCorners,
                smoothed
            );
        }

        /// <summary>
        /// Calculates the base release factor without smoothing.
        /// This is the core algorithm that determines release timing difficulty.
        /// </summary>
        private static double[] calculateBaseReleaseFactor(ManiaDifficultyContext data)
        {
            int timePoints = data.CornerData.BaseTimeCorners.Length;
            double[] releaseFactorBase = new double[timePoints];

            int longNoteTailCount = data.LongNoteTails.Count;
            if (longNoteTailCount == 0) return releaseFactorBase;

            // Calculate individual release difficulties for each long note
            double[] individualReleaseDifficulties = calculateIndividualReleaseDifficulties(data);

            // Process consecutive pairs of long note tails to determine release timing difficulty
            processConsecutiveLongNoteTails(data, individualReleaseDifficulties, releaseFactorBase, timePoints);

            return releaseFactorBase;
        }

        /// <summary>
        /// Calculates individual difficulty values for each long note release.
        /// This considers the note length and timing to the next note in the same column.
        /// </summary>
        private static double[] calculateIndividualReleaseDifficulties(ManiaDifficultyContext data)
        {
            double[] releaseDifficulties = new double[data.LongNoteTails.Count];

            for (int i = 0; i < data.LongNoteTails.Count; i++)
            {
                var tailNote = data.LongNoteTails[i];
                var nextNoteInColumn = tailNote.NextInColumn();

                // Calculate difficulty components
                double holdDuration = Math.Abs(tailNote.EndTime - tailNote.StartTime - 80.0);

                // Handle case where there's no next note in the column
                double releaseToNextNote;

                if (nextNoteInColumn is not null)
                {
                    releaseToNextNote = Math.Abs(nextNoteInColumn.StartTime - tailNote.EndTime - 80.0);
                }
                else
                {
                    // If no next note, use a default large value or base it on hold duration
                    releaseToNextNote = Math.Max(1000.0, holdDuration * 2.0);
                }

                // Normalize by hit leniency and scale
                double holdDifficultyComponent = 0.001 * holdDuration / data.HitLeniency;
                double timingDifficultyComponent = 0.001 * releaseToNextNote / data.HitLeniency;

                double lh = DifficultyCalculationUtils.Logistic(holdDifficultyComponent, 0.75, 5.0);
                double lt = DifficultyCalculationUtils.Logistic(timingDifficultyComponent, 0.75, 5.0);

                // harmonic mean of the two logistic outputs (safe against repeated evaluation)
                releaseDifficulties[i] = 2.0 * (lh * lt) / (lh + lt);
            }

            return releaseDifficulties;
        }

        /// <summary>
        /// Processes consecutive long note tails to determine release timing difficulty.
        /// The difficulty comes from coordinating multiple releases in sequence.
        /// </summary>
        private static void processConsecutiveLongNoteTails(ManiaDifficultyContext data, double[] releaseDifficulties, double[] releaseFactorBase, int timePoints)
        {
            for (int i = 0; i + 1 < data.LongNoteTails.Count; i++)
            {
                var currentTail = data.LongNoteTails[i];
                var nextTail = data.LongNoteTails[i + 1];

                // Find time range for this release sequence
                int startIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, currentTail.EndTime);
                int endIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, nextTail.EndTime);
                if (endIndex <= startIndex) continue;

                // Calculate release intensity based on timing and individual difficulties
                double releaseTimeDelta = 0.001 * (nextTail.EndTime - currentTail.EndTime);
                double baseReleaseIntensity = 0.08 * (1.0 / Math.Sqrt(releaseTimeDelta)) * (1.0 / data.HitLeniency);

                // Apply difficulty multiplier based on individual release difficulties
                double difficultyMultiplier = 1.0 + 0.8 * (releaseDifficulties[i] + releaseDifficulties[i + 1]);
                double finalReleaseIntensity = baseReleaseIntensity * difficultyMultiplier;

                // Apply to the time range
                for (int timeIndex = startIndex; timeIndex < endIndex && timeIndex < timePoints; timeIndex++)
                    releaseFactorBase[timeIndex] = finalReleaseIntensity;
            }
        }
    }
}
