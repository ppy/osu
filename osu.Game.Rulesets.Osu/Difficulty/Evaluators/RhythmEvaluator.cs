﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class RhythmEvaluator
    {
        private const int history_time_max = 5 * 1000; // 5 seconds
        private const int history_objects_max = 32;
        private const double rhythm_overall_multiplier = 0.95;
        private const double rhythm_ratio_multiplier = 12.0;

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            double rhythmComplexitySum = 0;

            double deltaDifferenceEpsilon = ((OsuDifficultyHitObject)current).HitWindowGreat * 0.3;

            var island = new Island(deltaDifferenceEpsilon);
            var previousIsland = new Island(deltaDifferenceEpsilon);

            // we can't use dictionary here because we need to compare island with a tolerance
            // which is impossible to pass into the hash comparer
            var islandCounts = new List<(Island Island, int Count)>();

            double startRatio = 0; // store the ratio of the current start of an island to buff for tighter rhythms

            bool firstDeltaSwitch = false;

            int historicalNoteCount = Math.Min(current.Index, history_objects_max);

            int rhythmStart = 0;

            while (rhythmStart < historicalNoteCount - 2 && current.StartTime - current.Previous(rhythmStart).StartTime < history_time_max)
                rhythmStart++;

            OsuDifficultyHitObject prevObj = (OsuDifficultyHitObject)current.Previous(rhythmStart);
            OsuDifficultyHitObject lastObj = (OsuDifficultyHitObject)current.Previous(rhythmStart + 1);

            // we go from the furthest object back to the current one
            for (int i = rhythmStart; i > 0; i--)
            {
                OsuDifficultyHitObject currObj = (OsuDifficultyHitObject)current.Previous(i - 1);

                // scales note 0 to 1 from history to now
                double timeDecay = (history_time_max - (current.StartTime - currObj.StartTime)) / history_time_max;
                double noteDecay = (double)(historicalNoteCount - i) / historicalNoteCount;

                double currHistoricalDecay = Math.Min(noteDecay, timeDecay); // either we're limited by time or limited by object count.

                double currDelta = currObj.StrainTime;
                double prevDelta = prevObj.StrainTime;
                double lastDelta = lastObj.StrainTime;

                // calculate how much current delta difference deserves a rhythm bonus
                // this function is meant to reduce rhythm bonus for deltas that are multiples of each other (i.e 100 and 200)
                double deltaDifferenceRatio = Math.Min(prevDelta, currDelta) / Math.Max(prevDelta, currDelta);
                double currRatio = 1.0 + rhythm_ratio_multiplier * Math.Min(0.5, Math.Pow(Math.Sin(Math.PI / deltaDifferenceRatio), 2));

                // reduce ratio bonus if delta difference is too big
                double fraction = Math.Max(prevDelta / currDelta, currDelta / prevDelta);
                double fractionMultiplier = Math.Clamp(2.0 - fraction / 8.0, 0.0, 1.0);

                double windowPenalty = Math.Min(1, Math.Max(0, Math.Abs(prevDelta - currDelta) - deltaDifferenceEpsilon) / deltaDifferenceEpsilon);

                double effectiveRatio = windowPenalty * currRatio * fractionMultiplier;

                if (firstDeltaSwitch)
                {
                    if (Math.Abs(prevDelta - currDelta) < deltaDifferenceEpsilon)
                    {
                        // island is still progressing
                        island.AddDelta((int)currDelta);
                    }
                    else
                    {
                        // bpm change is into slider, this is easy acc window
                        if (currObj.BaseObject is Slider)
                            effectiveRatio *= 0.125;

                        // bpm change was from a slider, this is easier typically than circle -> circle
                        // unintentional side effect is that bursts with kicksliders at the ends might have lower difficulty than bursts without sliders
                        if (prevObj.BaseObject is Slider)
                            effectiveRatio *= 0.3;

                        // repeated island polarity (2 -> 4, 3 -> 5)
                        if (island.IsSimilarPolarity(previousIsland))
                            effectiveRatio *= 0.5;

                        // previous increase happened a note ago, 1/1->1/2-1/4, dont want to buff this.
                        if (lastDelta > prevDelta + deltaDifferenceEpsilon && prevDelta > currDelta + deltaDifferenceEpsilon)
                            effectiveRatio *= 0.125;

                        // repeated island size (ex: triplet -> triplet)
                        // TODO: remove this nerf since its staying here only for balancing purposes because of the flawed ratio calculation
                        if (previousIsland.DeltaCount == island.DeltaCount)
                            effectiveRatio *= 0.5;

                        var islandCount = islandCounts.FirstOrDefault(x => x.Island.Equals(island));

                        if (islandCount != default)
                        {
                            int countIndex = islandCounts.IndexOf(islandCount);

                            // only add island to island counts if they're going one after another
                            if (previousIsland.Equals(island))
                                islandCount.Count++;

                            // repeated island (ex: triplet -> triplet)
                            double power = logistic(island.Delta, 2.75, 0.24, 14);
                            effectiveRatio *= Math.Min(3.0 / islandCount.Count, Math.Pow(1.0 / islandCount.Count, power));

                            islandCounts[countIndex] = (islandCount.Island, islandCount.Count);
                        }
                        else
                        {
                            islandCounts.Add((island, 1));
                        }

                        // scale down the difficulty if the object is doubletappable
                        double doubletapness = prevObj.GetDoubletapness(currObj);
                        effectiveRatio *= 1 - doubletapness * 0.75;

                        rhythmComplexitySum += Math.Sqrt(effectiveRatio * startRatio) * currHistoricalDecay;

                        startRatio = effectiveRatio;

                        previousIsland = island;

                        if (prevDelta + deltaDifferenceEpsilon < currDelta) // we're slowing down, stop counting
                            firstDeltaSwitch = false; // if we're speeding up, this stays true and we keep counting island size.

                        island = new Island((int)currDelta, deltaDifferenceEpsilon);
                    }
                }
                else if (prevDelta > currDelta + deltaDifferenceEpsilon) // we're speeding up
                {
                    // Begin counting island until we change speed again.
                    firstDeltaSwitch = true;

                    // bpm change is into slider, this is easy acc window
                    if (currObj.BaseObject is Slider)
                        effectiveRatio *= 0.6;

                    // bpm change was from a slider, this is easier typically than circle -> circle
                    // unintentional side effect is that bursts with kicksliders at the ends might have lower difficulty than bursts without sliders
                    if (prevObj.BaseObject is Slider)
                        effectiveRatio *= 0.6;

                    startRatio = effectiveRatio;

                    island = new Island((int)currDelta, deltaDifferenceEpsilon);
                }

                lastObj = prevObj;
                prevObj = currObj;
            }

            return Math.Sqrt(4 + rhythmComplexitySum * rhythm_overall_multiplier) / 2.0; // produces multiplier that can be applied to strain. range [1, infinity) (not really though)
        }

        private static double logistic(double x, double maxValue, double multiplier, double offset) => (maxValue / (1 + Math.Pow(Math.E, offset - (multiplier * x))));

        private class Island : IEquatable<Island>
        {
            private readonly double deltaDifferenceEpsilon;

            public Island(double epsilon)
            {
                deltaDifferenceEpsilon = epsilon;
            }

            public Island(int delta, double epsilon)
            {
                deltaDifferenceEpsilon = epsilon;
                Delta = Math.Max(delta, OsuDifficultyHitObject.MIN_DELTA_TIME);
                DeltaCount++;
            }

            public int Delta { get; private set; } = int.MaxValue;
            public int DeltaCount { get; private set; }

            public void AddDelta(int delta)
            {
                if (Delta == int.MaxValue)
                    Delta = Math.Max(delta, OsuDifficultyHitObject.MIN_DELTA_TIME);

                DeltaCount++;
            }

            public bool IsSimilarPolarity(Island other)
            {
                // TODO: consider islands to be of similar polarity only if they're having the same average delta (we don't want to consider 3 singletaps similar to a triple)
                //       naively adding delta check here breaks _a lot_ of maps because of the flawed ratio calculation
                return DeltaCount % 2 == other.DeltaCount % 2;
            }

            public bool Equals(Island? other)
            {
                if (other == null)
                    return false;

                return Math.Abs(Delta - other.Delta) < deltaDifferenceEpsilon &&
                       DeltaCount == other.DeltaCount;
            }

            public override string ToString()
            {
                return $"{Delta}x{DeltaCount}";
            }
        }
    }
}
