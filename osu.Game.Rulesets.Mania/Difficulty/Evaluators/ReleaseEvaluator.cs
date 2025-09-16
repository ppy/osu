// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Strain;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public static class ReleaseFactorEvaluator
    {
        /// <summary>
        /// Evaluates the release timing difficulty at a specific time point.
        /// </summary>
        public static double EvaluateDifficultyAt(double time, SunnyStrainData data)
        {
            return data.SampleFeatureAtTime(time, data.ReleaseFactor);
        }

        /// <summary>
        /// Computes the release factor values across all time points.
        /// This focuses on the difficulty of timing long note releases correctly.
        /// </summary>
        public static double[] ComputeReleaseFactor(SunnyStrainData data)
        {
            double[] baseRelease = calculateBaseReleaseFactor(data);

            // Apply smoothing to create stable difficulty curves
            double[] smoothed = StrainArrayUtils.ApplySmoothingToArray(
                data.CornerData.BaseTimeCorners,
                baseRelease,
                data.Config.smoothingWindowMs,
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
        private static double[] calculateBaseReleaseFactor(SunnyStrainData data)
        {
            int timePoints = data.CornerData.BaseTimeCorners.Length;
            double[] releaseFactorBase = new double[timePoints];

            // Build lookup tables for efficient note searching by column
            int[][] startTimesByColumn = buildColumnStartTimeLookup(data);

            int longNoteTailCount = data.LongNoteTails.Length;
            if (longNoteTailCount == 0) return releaseFactorBase;

            // Calculate individual release difficulties for each long note
            double[] individualReleaseDifficulties = calculateIndividualReleaseDifficulties(data, startTimesByColumn);

            // Process consecutive pairs of long note tails to determine release timing difficulty
            processConsecutiveLongNoteTails(data, individualReleaseDifficulties, releaseFactorBase, timePoints);

            return releaseFactorBase;
        }

        /// <summary>
        /// Builds lookup tables of note start times organized by column for efficient searching.
        /// </summary>
        private static int[][] buildColumnStartTimeLookup(SunnyStrainData data)
        {
            int[][] startTimesByColumn = new int[data.KeyCount][];

            for (int column = 0; column < data.KeyCount; column++)
            {
                var columnNotes = data.NotesByColumn[column];
                int[] startTimes = new int[columnNotes.Length];

                for (int i = 0; i < columnNotes.Length; i++)
                    startTimes[i] = columnNotes[i].StartTime;

                startTimesByColumn[column] = startTimes;
            }

            return startTimesByColumn;
        }

        /// <summary>
        /// Calculates individual difficulty values for each long note release.
        /// This considers the note length and timing to the next note in the same column.
        /// </summary>
        private static double[] calculateIndividualReleaseDifficulties(SunnyStrainData data, int[][] startTimesByColumn)
        {
            double[] releaseDifficulties = new double[data.LongNoteTails.Length];

            for (int i = 0; i < data.LongNoteTails.Length; i++)
            {
                var tailNote = data.LongNoteTails[i];
                var nextNoteInColumn = findNextNoteInColumn(tailNote, data, startTimesByColumn);

                // Calculate difficulty components
                double holdDuration = Math.Abs(tailNote.EndTime - tailNote.StartTime - 80.0);
                double releaseToNextNote = Math.Abs(nextNoteInColumn.StartTime - tailNote.EndTime - 80.0);

                // Normalize by hit leniency and scale
                double holdDifficultyComponent = 0.001 * holdDuration / data.HitLeniency;
                double timingDifficultyComponent = 0.001 * releaseToNextNote / data.HitLeniency;

                // Combine using logistic function to create smooth difficulty curve
                releaseDifficulties[i] = 2.0 / (2.0 + Math.Exp(-5.0 * (holdDifficultyComponent - 0.75)) +
                                                Math.Exp(-5.0 * (timingDifficultyComponent - 0.75)));
            }

            return releaseDifficulties;
        }

        /// <summary>
        /// Processes consecutive long note tails to determine release timing difficulty.
        /// The difficulty comes from coordinating multiple releases in sequence.
        /// </summary>
        private static void processConsecutiveLongNoteTails(SunnyStrainData data, double[] releaseDifficulties, double[] releaseFactorBase, int timePoints)
        {
            for (int i = 0; i + 1 < data.LongNoteTails.Length; i++)
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

        /// <summary>
        /// Finds the next note in the same column as the given long note.
        /// This is used to determine release timing difficulty.
        /// </summary>
        private static SunnyPreprocessor.Note findNextNoteInColumn(SunnyPreprocessor.Note longNote, SunnyStrainData data, int[][] startTimesByColumn)
        {
            if (longNote.Column < 0 || longNote.Column >= data.KeyCount)
                return new SunnyPreprocessor.Note(0, (int)1e9, (int)1e9); // Invalid note placeholder

            int[] columnStartTimes = startTimesByColumn[longNote.Column];
            int searchIndex = Array.BinarySearch(columnStartTimes, longNote.StartTime);
            if (searchIndex < 0) searchIndex = ~searchIndex;

            var columnNotes = data.NotesByColumn[longNote.Column];
            if (searchIndex + 1 < columnNotes.Length)
                return columnNotes[searchIndex + 1];

            return new SunnyPreprocessor.Note(0, (int)1e9, (int)1e9); // No next note found
        }
    }
}
