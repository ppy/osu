// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public static class PressingIntensityEvaluator
    {
        /// <summary>
        /// Evaluates the pressing intensity difficulty at a specific time point.
        /// </summary>
        public static double EvaluateDifficultyAt(double time, SunnyStrainData data)
        {
            return data.SampleFeatureAtTime(time, data.PressingIntensity);
        }

        /// <summary>
        /// Computes the pressing intensity values across all time points.
        /// This considers long note density, anchor patterns, and note timing.
        /// </summary>
        public static double[] ComputePressingIntensity(SunnyStrainData data)
        {
            var longNoteDensity = buildLongNoteDensity(data);
            double[] anchorValues = CalculateAnchorValues(data);
            double[] basePattern = calculateBasePressingIntensity(data, longNoteDensity, anchorValues);

            // Apply smoothing to create more stable difficulty curves
            double[] smoothed = StrainArrayUtils.ApplySmoothingToArray(
                data.CornerData.BaseTimeCorners,
                basePattern,
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
        /// Builds density data for long notes, which affects how difficult pressing patterns are.
        /// Long notes create sustained finger usage that impacts coordination.
        /// </summary>
        private static LongNoteDensityData buildLongNoteDensity(SunnyStrainData data)
        {
            if (data.LongNotes.Length == 0)
            {
                return new LongNoteDensityData
                {
                    TimePoints = new[] { 0.0, data.MaxTime },
                    DensityValues = new[] { 0.0 },
                    CumulativeSum = new[] { 0.0 }
                };
            }

            int longNoteCount = data.LongNotes.Length;
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
                densityDeltas[eventPosition++] = -1.0; // Full removal at end
            }

            eventTimes[eventPosition] = 0;
            densityDeltas[eventPosition++] = 0.0;
            eventTimes[eventPosition] = data.MaxTime;
            densityDeltas[eventPosition++] = 0.0;

            return processDensityEvents(eventTimes, densityDeltas, eventPosition);
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

            // Calculate cumulative sum for efficient range queries
            var cumulative = new List<double>(timePointsList.Count + 1) { 0.0 };

            for (int j = 0; j < timePointsList.Count - 1; j++)
            {
                double timeSpan = timePointsList[j + 1] - timePointsList[j];
                cumulative.Add(cumulative[^1] + timeSpan * densityValuesList[j]);
            }

            return new LongNoteDensityData
            {
                TimePoints = timePointsList.ToArray(),
                DensityValues = densityValuesList.ToArray(),
                CumulativeSum = cumulative.ToArray()
            };
        }

        /// <summary>
        /// Calculates anchor values that represent how balanced finger usage is.
        /// </summary>
        public static double[] CalculateAnchorValues(SunnyStrainData data)
        {
            double[][] keyUsage400 = calculateKeyUsage400(data);
            return computeAnchorFromKeyUsage(keyUsage400);
        }

        /// <summary>
        /// Calculates key usage with a 400ms window around each note.
        /// This determines how much each finger is being used at different times.
        /// </summary>
        private static double[][] calculateKeyUsage400(SunnyStrainData data)
        {
            int timePoints = data.CornerData.BaseTimeCorners.Length;
            if (timePoints == 0) return Array.Empty<double[]>();

            double[][] keyUsage = new double[data.KeyCount][];
            for (int column = 0; column < data.KeyCount; column++)
                keyUsage[column] = new double[timePoints];

            for (int noteIndex = 0; noteIndex < data.AllNotes.Length; noteIndex++)
            {
                var note = data.AllNotes[noteIndex];
                int noteStartTime = (int)Math.Round(note.StartTime);
                int noteEndTime = (int)Math.Round(note.EndTime); // note.IsLong ? (int)Math.Round(note.EndTime) : noteStartTime;

                int leftWindow400Index = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners,
                    noteStartTime - data.Config.keyUsageWindowMs);
                int leftIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, noteStartTime);
                int rightIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, noteEndTime);
                int rightWindow400Index = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners,
                    noteEndTime + data.Config.keyUsageWindowMs);

                // Base usage value depends on note duration
                double baseUsage = 3.75 + Math.Min(noteEndTime - noteStartTime, 1500) / 150.0;

                int column = note.Column;
                if (column < 0 || column >= data.KeyCount) continue;

                double[] columnUsage = keyUsage[column];

                applyFullUsage(columnUsage, leftIndex, rightIndex, timePoints, baseUsage);

                // Gradual falloff in the windows before and after the note
                double windowDenominator = data.Config.keyUsageWindowMs * data.Config.keyUsageWindowMs;
                applyWindowUsage(columnUsage, data.CornerData.BaseTimeCorners, leftWindow400Index, leftIndex,
                    noteStartTime, baseUsage, windowDenominator, timePoints);
                applyWindowUsage(columnUsage, data.CornerData.BaseTimeCorners, rightIndex, rightWindow400Index,
                    noteEndTime, baseUsage, windowDenominator, timePoints);
            }

            return keyUsage;
        }

        /// <summary>
        /// Applies full usage value during the note duration.
        /// </summary>
        private static void applyFullUsage(double[] columnUsage, int startIndex, int endIndex, int timePoints, double baseUsage)
        {
            for (int i = startIndex; i < endIndex && i < timePoints; i++)
                columnUsage[i] += baseUsage;
        }

        /// <summary>
        /// Applies gradually decreasing usage in the time windows around notes.
        /// </summary>
        private static void applyWindowUsage(double[] columnUsage, double[] baseTimeCorners, int windowStart, int windowEnd, int referenceTime, double baseUsage, double denominator, int timePoints)
        {
            for (int i = windowStart; i < windowEnd && i < timePoints; i++)
            {
                if (i >= 0)
                {
                    double timeDifference = Math.Abs(baseTimeCorners[i] - referenceTime);
                    double reducedUsage = baseUsage - baseUsage / denominator * (timeDifference * timeDifference);
                    columnUsage[i] += reducedUsage;
                }
            }
        }

        /// <summary>
        /// Computes anchor values from key usage patterns.
        /// Anchor represents how well-balanced the finger usage is - better balance = easier to play accurately.
        /// </summary>
        private static double[] computeAnchorFromKeyUsage(double[][] keyUsage400)
        {
            int timePoints = keyUsage400.Length == 0 ? 0 : keyUsage400[0].Length;
            if (timePoints == 0) return Array.Empty<double>();

            double[] anchorValues = new double[timePoints];
            double[] columnUsageBuffer = new double[keyUsage400.Length];

            for (int timeIndex = 0; timeIndex < timePoints; timeIndex++)
            {
                for (int column = 0; column < keyUsage400.Length; column++)
                    columnUsageBuffer[column] = keyUsage400[column][timeIndex];

                Array.Sort(columnUsageBuffer);
                Array.Reverse(columnUsageBuffer);

                int activeColumnCount = 0;

                for (int i = 0; i < columnUsageBuffer.Length; i++)
                {
                    if (columnUsageBuffer[i] != 0.0)
                        activeColumnCount++;
                }

                double anchorValue = 0.0;

                if (activeColumnCount > 1)
                {
                    // Calculate balance score based on usage ratios between adjacent columns
                    double walkSum = 0.0;
                    double maxWalkSum = 0.0;

                    for (int i = 0; i < activeColumnCount - 1; i++)
                    {
                        double currentUsage = columnUsageBuffer[i];
                        double nextUsage = columnUsageBuffer[i + 1];
                        if (currentUsage == 0.0) break;

                        double ratio = nextUsage / currentUsage;
                        double difference = 0.5 - ratio; // How far from perfect balance (0.5 ratio)
                        double balanceFactor = 1.0 - 4.0 * (difference * difference); // Penalty for imbalance

                        walkSum += currentUsage * balanceFactor;
                        maxWalkSum += currentUsage;
                    }

                    anchorValue = maxWalkSum != 0.0 ? walkSum / maxWalkSum : 0.0;
                }

                // Apply non-linear scaling to emphasize good anchoring
                double adjustedValue = anchorValue - 0.22;
                double cubicTerm = adjustedValue * adjustedValue * adjustedValue;
                anchorValues[timeIndex] = 1.0 + Math.Min(anchorValue - 0.18, 5.0 * cubicTerm);
            }

            return anchorValues;
        }

        /// <summary>
        /// Calculates the base pressing intensity before smoothing.
        /// This is the core calculation that determines how difficult pressing patterns are.
        /// </summary>
        private static double[] calculateBasePressingIntensity(SunnyStrainData data, LongNoteDensityData longNoteDensity, double[] anchorValues)
        {
            int timePoints = data.CornerData.BaseTimeCorners.Length;
            double[] pressingIntensityBase = new double[timePoints];

            for (int noteIndex = 0; noteIndex + 1 < data.AllNotes.Length; noteIndex++)
            {
                var currentNote = data.AllNotes[noteIndex];
                var nextNote = data.AllNotes[noteIndex + 1];
                double deltaTime = nextNote.StartTime - currentNote.StartTime;

                // Handle simultaneous notes (chords)
                if (deltaTime == 0)
                {
                    processSimultaneousNotes(currentNote, data, pressingIntensityBase, timePoints);
                    continue;
                }

                // Process normal note sequence
                processNoteSequence(currentNote, nextNote, deltaTime, data, longNoteDensity, anchorValues,
                    pressingIntensityBase, timePoints);
            }

            return pressingIntensityBase;
        }

        /// <summary>
        /// Processes simultaneous notes (chords), which create difficulty spikes.
        /// </summary>
        private static void processSimultaneousNotes(ManiaDifficultyHitObject currentNote, SunnyStrainData data, double[] pressingIntensityBase, int timePoints)
        {
            // Calculate chord difficulty based on timing window
            double chordDifficulty = 1000.0 * Math.Pow(0.02 * (4.0 / data.HitLeniency - 24.0), 0.25);

            int timeIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, currentNote.StartTime);
            if (timeIndex >= 0 && timeIndex < timePoints)
                pressingIntensityBase[timeIndex] += chordDifficulty;
        }

        /// <summary>
        /// Processes a sequence of two consecutive notes to calculate pressing difficulty.
        /// </summary>
        private static void processNoteSequence(ManiaDifficultyHitObject currentNote, ManiaDifficultyHitObject nextNote, double deltaTime, SunnyStrainData data, LongNoteDensityData longNoteDensity, double[] anchorValues, double[] pressingIntensityBase, int timePoints)
        {
            int startIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, currentNote.StartTime);
            int endIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, nextNote.StartTime);
            if (endIndex <= startIndex) return;

            double deltaTimeSeconds = 0.001 * deltaTime;

            // Calculate various difficulty components
            double longNoteBoost = calculateLongNoteBoost(currentNote, nextNote, longNoteDensity);
            double streamBoost = calculateStreamBoost(deltaTimeSeconds, data);
            double pressingValue = calculatePressingValue(deltaTimeSeconds, longNoteBoost, streamBoost, data);

            // Apply the calculated difficulty to the time range
            applyPressingValueToRange(pressingValue, anchorValues, pressingIntensityBase, startIndex, endIndex, timePoints);
        }

        /// <summary>
        /// Calculates boost factor from long note density in the local area.
        /// </summary>
        private static double calculateLongNoteBoost(ManiaDifficultyHitObject currentNote, ManiaDifficultyHitObject nextNote, LongNoteDensityData longNoteDensity)
        {
            double longNoteDensitySum = longNoteDensity.SumBetween((int)currentNote.StartTime, (int)nextNote.StartTime);
            return 1.0 + 6.0 * 0.001 * longNoteDensitySum;
        }

        /// <summary>
        /// Applies the calculated pressing value to a time range, considering anchor multipliers.
        /// </summary>
        private static void applyPressingValueToRange(double pressingValue, double[] anchorValues, double[] pressingIntensityBase, int startIndex, int endIndex, int timePoints)
        {
            for (int index = startIndex; index < endIndex && index < timePoints; index++)
            {
                double anchorMultiplier = anchorValues[index];

                // Apply anchor multiplier with capping to prevent extreme values
                double finalIntensity = Math.Min(
                    pressingValue * anchorMultiplier,
                    Math.Max(pressingValue, pressingValue * 2.0 - 10.0)
                );

                pressingIntensityBase[index] += finalIntensity;
            }
        }

        /// <summary>
        /// Calculates the base pressing difficulty value considering timing, context, and hit leniency.
        /// </summary>
        private static double calculatePressingValue(double deltaTime, double longNoteBoost, double streamBoost, SunnyStrainData data)
        {
            double baseIntensity = 1.0 / deltaTime;
            double inverseLeniency = 1.0 / data.HitLeniency;
            double leniencyFactor = Math.Sqrt(Math.Sqrt(0.08 * inverseLeniency));

            // Calculate timing penalty based on how the delta relates to hit leniency
            double timingPenalty = calculateTimingPenalty(deltaTime, data.HitLeniency, inverseLeniency);

            // Use the higher of stream boost or long note boost
            double contextMultiplier = Math.Max(streamBoost, longNoteBoost);

            return baseIntensity * leniencyFactor * timingPenalty * contextMultiplier;
        }

        /// <summary>
        /// Calculates penalty for notes that are too close together relative to the hit window.
        /// </summary>
        private static double calculateTimingPenalty(double deltaTime, double hitLeniency, double inverseLeniency)
        {
            if (deltaTime < 2.0 * hitLeniency / 3.0)
            {
                // Very close notes - calculate based on distance from center of hit window
                double windowDeviation = deltaTime - hitLeniency / 2.0;
                double penaltyFactor = Math.Max(0.0, 1.0 - 24.0 * inverseLeniency * windowDeviation * windowDeviation);
                return Math.Sqrt(Math.Sqrt(penaltyFactor));
            }
            else
            {
                // Normal spacing - use standard deviation calculation
                double standardDeviation = hitLeniency / 6.0;
                double penaltyFactor = Math.Max(0.0, 1.0 - 24.0 * inverseLeniency * standardDeviation * standardDeviation);
                return Math.Sqrt(Math.Sqrt(penaltyFactor));
            }
        }

        /// <summary>
        /// Calculates boost factor for stream patterns (rapid note sequences).
        /// Stream patterns get bonus difficulty due to stamina and coordination requirements.
        /// </summary>
        private static double calculateStreamBoost(double deltaTime, SunnyStrainData data)
        {
            double streamRatio = 7.5 / deltaTime; // How "streamy" this pattern is

            if (streamRatio > data.Config.streamBoostMinRatio && streamRatio < data.Config.streamBoostMaxRatio)
            {
                double ratioDistance = streamRatio - data.Config.streamBoostMaxRatio;
                double quadraticFactor = ratioDistance * ratioDistance;

                return 1.0 + data.Config.streamBoostCoefficient * (streamRatio - data.Config.streamBoostMinRatio) * quadraticFactor;
            }

            return 1.0;
        }
    }
}
