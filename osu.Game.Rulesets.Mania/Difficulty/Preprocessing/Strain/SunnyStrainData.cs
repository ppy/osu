// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner;
using osu.Game.Rulesets.Mania.Difficulty.Skills;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Strain
{
    public class SunnyStrainData
    {
        /// <summary>All notes in the beatmap, sorted by start time</summary>
        public SunnyPreprocessor.Note[] AllNotes { get; set; }

        /// <summary>Notes organized by column for efficient column-specific processing</summary>
        public SunnyPreprocessor.Note[][] NotesByColumn { get; set; }

        /// <summary>All long notes in the beatmap</summary>
        public SunnyPreprocessor.Note[] LongNotes { get; set; }

        /// <summary>Long notes sorted by end time (for release timing calculations)</summary>
        public SunnyPreprocessor.Note[] LongNoteTails { get; set; }

        /// <summary>Time corner data defining sampling points for difficulty calculation</summary>
        public CornerData CornerData { get; set; }

        /// <summary>TEMPORARY ONLY FOR TESTING</summary>
        public FormulaConfig Config { get; set; }

        /// <summary>Hit timing leniency based on Overall Difficulty</summary>
        public double HitLeniency { get; set; }

        /// <summary>Number of columns (keys) in this beatmap</summary>
        public int KeyCount { get; set; }

        /// <summary>Maximum time value in the beatmap</summary>
        public int MaxTime { get; set; }

        /// <summary>Difficulty from same-column patterns (jacks, stairs)</summary>
        public double[] SameColumnPressure { get; set; }

        /// <summary>Difficulty from cross-column coordination patterns</summary>
        public double[] CrossColumnPressure { get; set; }

        /// <summary>Difficulty from pressing intensity and timing</summary>
        public double[] PressingIntensity { get; set; }

        /// <summary>Difficulty penalty from uneven timing patterns</summary>
        public double[] Unevenness { get; set; }

        /// <summary>Difficulty from long note release timing</summary>
        public double[] ReleaseFactor { get; set; }

        /// <summary>Local note count for density calculations</summary>
        public double[] LocalNoteCount { get; set; }

        /// <summary>Number of active keys at each time point</summary>
        public double[] ActiveKeyCount { get; set; }

        /// <summary>
        /// Samples a difficulty feature value at a specific time using interpolation.
        /// This is the main interface for getting difficulty values at arbitrary time points.
        /// </summary>
        public double SampleFeatureAtTime(double time, double[] featureArray)
            => interpolateValue(time, CornerData.TimeCorners, featureArray);

        /// <summary>
        /// Performs linear interpolation to find a value at a specific point.
        /// This enables smooth difficulty curves between discrete calculation points.
        /// </summary>
        private double interpolateValue(double targetPoint, double[] xArray, double[] yArray)
        {
            if (xArray.Length == 0 || yArray.Length == 0)
                return 0.0;

            // Handle boundary cases
            if (targetPoint <= xArray[0]) return yArray[0];

            int lastIndex = xArray.Length - 1;
            if (targetPoint >= xArray[lastIndex]) return yArray[lastIndex];

            // Find the appropriate interval using binary search
            int searchResult = Array.BinarySearch(xArray, targetPoint);
            if (searchResult >= 0) return yArray[searchResult];

            // Convert negative result to insertion point
            int rightIndex = ~searchResult;
            int leftIndex = rightIndex - 1;

            // Perform linear interpolation
            double denominator = (xArray[rightIndex] - xArray[leftIndex]);
            if (denominator == 0.0) return yArray[leftIndex]; // Avoid division by zero

            double interpolationFactor = (targetPoint - xArray[leftIndex]) / denominator;
            return yArray[leftIndex] * (1.0 - interpolationFactor) + yArray[rightIndex] * interpolationFactor;
        }
    }
}
