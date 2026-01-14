// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.UtilityPreprocessing
{
    public class LongNoteDensityPreprocessor
    {
        /// <summary>
        /// Builds density data for long notes, which affects how difficult pressing patterns are.
        /// Long notes create sustained finger usage that impacts coordination.
        /// </summary>
        public static void ProcessAndAssign(ManiaDifficultyData data)
        {
            var longNoteDensity = new LongNoteDensityData
            {
                TimePoints = new[] { 0.0, data.MaxTime },
                DensityValues = new[] { 0.0 },
            };

            if (data.LongNotes.Count == 0)
            {
                data.LongNoteDensityData = longNoteDensity;
                return;
            }

            int longNoteCount = data.LongNotes.Count;
            int estimatedEventCount = longNoteCount * 3 + 4; // Each long note creates ~3 events

            double[] eventTimes = new double[estimatedEventCount];
            double[] densityDeltas = new double[estimatedEventCount];
            int eventPosition = 0;

            for (int i = 0; i < longNoteCount; i++)
            {
                var longNote = data.LongNotes[i];

                // Create density curve: ramp up quickly, then decay
                double earlyPoint = Math.Min(longNote.StartTime + 60, longNote.EndTime);
                double midPoint = Math.Min(longNote.StartTime + 120, longNote.EndTime);

                // Add density at different points of the long note
                eventTimes[eventPosition] = earlyPoint;
                densityDeltas[eventPosition++] = 1.3; // Strong initial impact

                eventTimes[eventPosition] = midPoint;
                densityDeltas[eventPosition++] = -0.3; // Slight reduction

                eventTimes[eventPosition] = longNote.EndTime;
                densityDeltas[eventPosition++] = -1.0; // Full removal at the end
            }

            eventTimes[eventPosition] = 0;
            densityDeltas[eventPosition++] = 0.0;
            eventTimes[eventPosition] = data.MaxTime;
            densityDeltas[eventPosition++] = 0.0;

            data.LongNoteDensityData = processDensityEvents(eventTimes, densityDeltas, eventPosition);
        }

        /// <summary>
        /// Processes density events to create a continuous density function.
        /// </summary>
        private static LongNoteDensityData processDensityEvents(double[] eventTimes, double[] densityDeltas, int eventCount)
        {
            int[] eventIndices = new int[eventCount];
            for (int i = 0; i < eventCount; i++) eventIndices[i] = i;
            Array.Sort(eventIndices, 0, eventCount, Comparer<int>.Create((a, b) => eventTimes[a].CompareTo(eventTimes[b])));

            var timePointsList = new List<double>(eventCount);
            var densityValuesList = new List<double>(eventCount);

            double currentDensity = 0.0;
            int eventIndex = 0;

            while (eventIndex < eventCount)
            {
                double currentTime = eventTimes[eventIndices[eventIndex]];

                // Apply all events at this time
                while (eventIndex < eventCount && eventTimes[eventIndices[eventIndex]] == currentTime)
                {
                    currentDensity += densityDeltas[eventIndices[eventIndex]];
                    eventIndex++;
                }

                // Clamp density to reasonable values
                double clampedDensity = Math.Min(currentDensity, 2.5 + 0.5 * currentDensity);
                timePointsList.Add(currentTime);
                densityValuesList.Add(clampedDensity);
            }

            /*
             var cumulative = new List<double>(timePointsList.Count + 1) { 0.0 };

            for (int j = 0; j < timePointsList.Count - 1; j++)
            {
                double timeSpan = timePointsList[j + 1] - timePointsList[j];
                cumulative.Add(cumulative[^1] + timeSpan * densityValuesList[j]);
            }
             */

            return new LongNoteDensityData
            {
                TimePoints = timePointsList.ToArray(),
                DensityValues = densityValuesList.ToArray(),
            };
        }
    }
}
