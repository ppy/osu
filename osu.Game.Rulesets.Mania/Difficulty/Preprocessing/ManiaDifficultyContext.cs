// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner.Data;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing
{
    public class ManiaDifficultyContext
    {
        public List<ManiaDifficultyHitObject> AllNotes { get; set; } = new List<ManiaDifficultyHitObject>();
        public List<ManiaDifficultyHitObject> LongNotes { get; set; } = new List<ManiaDifficultyHitObject>();
        public List<ManiaDifficultyHitObject> LongNoteTails { get; set; } = new List<ManiaDifficultyHitObject>();

        /// <summary>Time corner data defining sampling points for difficulty calculation</summary>
        public CornerData CornerData { get; set; } = new CornerData();

        /// <summary>Hit timing leniency based on Overall Difficulty</summary>
        public double HitLeniency { get; set; }

        public int KeyCount { get; set; }

        /// <summary>Maximum time value in the beatmap</summary>
        public int MaxTime { get; set; }

        /// <summary>Difficulty from same-column patterns (jacks)</summary>
        public double[] SameColumnPressure { get; set; } = Array.Empty<double>();

        /// <summary>Difficulty from cross-column coordination patterns</summary>
        public double[] CrossColumnPressure { get; set; } = Array.Empty<double>();

        /// <summary>Difficulty from pressing intensity and timing</summary>
        public double[] PressingIntensity { get; set; } = Array.Empty<double>();

        /// <summary>Difficulty penalty from uneven timing patterns</summary>
        public double[] Unevenness { get; set; } = Array.Empty<double>();

        /// <summary>Difficulty from long note release timing</summary>
        public double[] ReleaseFactor { get; set; } = Array.Empty<double>();

        /// <summary>Local note count for density calculations</summary>
        public double[] LocalNoteCount { get; set; } = Array.Empty<double>();

        /// <summary>Number of active keys at each time point</summary>
        public double[] ActiveKeyCount { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Samples a difficulty feature value at a specific time using interpolation.
        /// This is the main interface for getting difficulty values at arbitrary time points.
        /// </summary>
        public double SampleFeatureAtTime(double time, double[] featureArray) => interpolateValue(time, CornerData.TimeCorners, featureArray);

        private double interpolateValue(double targetTime, double[] timePoints, double[] values)
        {
            if (timePoints.Length == 0 || values.Length == 0)
                return 0.0;

            if (targetTime <= timePoints[0]) return values[0];
            if (targetTime >= timePoints[^1]) return values[^1];

            int index = Array.BinarySearch(timePoints, targetTime);

            if (index >= 0)
                return values[index];

            int floorIndex = (~index) - 1;
            return values[Math.Clamp(floorIndex, 0, values.Length - 1)];
        }
    }
}
