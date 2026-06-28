// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Speed
{
    public static class RhythmEvaluator
    {
        private const int history_time_max = 5 * 1000; // 5 seconds
        private const int history_objects_max = 32;
        private const double rhythm_overall_multiplier = 0.95;
        private const double rhythm_ratio_multiplier = 26.0;

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            double rhythmComplexitySum = 0;

            double deltaDifferenceEpsilon = ((OsuDifficultyHitObject)current).HitWindowGreat * 0.3;

            var island = new Island(int.MaxValue);
            var previousIsland = new Island(int.MaxValue);

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
            OsuDifficultyHitObject prevPrevObj = (OsuDifficultyHitObject)current.Previous(rhythmStart + 1);

            // we go from the furthest object back to the current one
            for (int i = rhythmStart; i > 0; i--)
            {
                OsuDifficultyHitObject currObj = (OsuDifficultyHitObject)current.Previous(i - 1);

                if (currObj.BaseObject is Spinner)
                    continue;

                // scales note 0 to 1 from history to now
                double timeDecay = (history_time_max - (current.StartTime - currObj.StartTime)) / history_time_max;
                double noteDecay = (double)(historicalNoteCount - i) / historicalNoteCount;

                double currHistoricalDecay = Math.Min(noteDecay, timeDecay); // either we're limited by time or limited by object count.

                // Use custom cap value to ensure that at this point delta time is actually zero
                const double delta_min_value = 1e-7;

                double currDelta = Math.Max(currObj.DeltaTime, delta_min_value);
                double prevDelta = Math.Max(prevObj.DeltaTime, delta_min_value);

                double currPrevDeltaDelta = Math.Abs(prevDelta - currDelta);

                // Make sure to always have the current island initialised - if we don't do it here it will only initialise on the next rhythm change
                if (island.Delta == int.MaxValue)
                    island = new Island((int)currDelta);

                // calculate how much current delta difference deserves a rhythm bonus
                // this function is meant to reduce rhythm bonus for deltas that are multiples of each other (i.e 100 and 200)
                double deltaDifference = Math.Max(prevDelta, currDelta) / Math.Min(prevDelta, currDelta);

                // reduce ratio bonus if delta difference is too big
                double differenceMultiplier = Math.Clamp(2.0 - deltaDifference / 8.0, 0.0, 1.0);

                double windowPenalty = Math.Clamp((currPrevDeltaDelta - deltaDifferenceEpsilon) / deltaDifferenceEpsilon, 0, 1);

                double effectiveRatio = getEffectiveRatio(deltaDifference) * windowPenalty * differenceMultiplier;

                // if previous object is a slider it might be easier to tap since you don't have to do a whole tapping motion
                // while a full deltatime might end up some weird ratio the "unpress->tap" motion might be simple
                // for example a slider-circle-circle pattern should be evaluated as a regular triple and not as a single->double
                if (prevObj.BaseObject is Slider)
                {
                    double sliderLazyEndDelta = currObj.MinimumJumpTime;
                    double sliderLazyDeltaDifference = Math.Max(sliderLazyEndDelta, currDelta) / Math.Min(sliderLazyEndDelta, currDelta);

                    double sliderRealEndDelta = currObj.LastObjectEndDeltaTime;
                    double sliderRealDeltaDifference = Math.Max(sliderRealEndDelta, currDelta) / Math.Min(sliderRealEndDelta, currDelta);

                    double sliderEffectiveRatio = Math.Min(getEffectiveRatio(sliderLazyDeltaDifference), getEffectiveRatio(sliderRealDeltaDifference));
                    effectiveRatio = Math.Min(sliderEffectiveRatio, effectiveRatio);
                }

                if (currPrevDeltaDelta < deltaDifferenceEpsilon)
                {
                    // island is still progressing
                    island.AddDelta((int)currDelta);
                }

                if (firstDeltaSwitch)
                {
                    if (currPrevDeltaDelta > deltaDifferenceEpsilon)
                    {
                        // bpm change is into slider, this is easy acc window
                        if (currObj.BaseObject is Slider)
                            effectiveRatio *= 0.5;

                        // repeated island polarity (2 -> 4, 3 -> 5)
                        if (island.IsSimilarPolarity(previousIsland, deltaDifferenceEpsilon))
                            effectiveRatio *= 0.5;

                        // previous increase happened a note ago, 1/1->1/2-1/4, dont want to buff this.
                        if (Math.Max(prevPrevObj.DeltaTime, delta_min_value) > prevDelta + deltaDifferenceEpsilon && prevDelta > currDelta + deltaDifferenceEpsilon)
                            effectiveRatio *= 0.125;

                        // repeated island size (ex: triplet -> triplet)
                        // TODO: remove this nerf since its staying here only for balancing purposes because of the flawed ratio calculation
                        if (previousIsland.DeltaCount == island.DeltaCount)
                            effectiveRatio *= 0.5;

                        bool isSpeedingUp = prevDelta > currDelta + deltaDifferenceEpsilon;

                        if (isSpeedingUp)
                            effectiveRatio *= 0.65;

                        bool found = false;

                        foreach ((Island Island, int Count) tuple in islandCounts)
                        {
                            if (tuple.Island.AlmostEquals(island, deltaDifferenceEpsilon))
                            {
                                int countIndex = islandCounts.IndexOf(tuple);
                                int count = tuple.Count;

                                // only add island to island counts if they're going one after another
                                if (previousIsland.AlmostEquals(island, deltaDifferenceEpsilon))
                                    islandCounts[countIndex] = (tuple.Island, ++count);

                                // repeated island (ex: triplet -> triplet)
                                double power = DiffUtils.Logistic(island.Delta, maxValue: 2.75, multiplier: 0.24, midpointOffset: 58.33);
                                effectiveRatio *= Math.Min(3.0 / count, DiffUtils.Pow(1.0 / count, power));

                                found = true;
                                break;
                            }
                        }

                        if (!found && island.DeltaCount > 0)
                            islandCounts.Add((island, 1));

                        // scale down the difficulty if the object is double-tappable
                        effectiveRatio *= 1 - prevObj.CalculateDoubleTapFeasibility(currObj) * 0.75;

                        if (island.DeltaCount > 1)
                        {
                            rhythmComplexitySum += Math.Sqrt(effectiveRatio * startRatio) * currHistoricalDecay;
                        }
                        else
                        {
                            // constant difficulty for single-note islands
                            rhythmComplexitySum += 0.7 * currHistoricalDecay;
                        }

                        startRatio = effectiveRatio;

                        if (prevDelta + deltaDifferenceEpsilon < currDelta) // we're slowing down, stop counting
                            firstDeltaSwitch = false; // if we're speeding up, this stays true and we keep counting island size.

                        previousIsland = island;
                        island = new Island((int)currDelta);
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

                    island = new Island((int)currDelta);
                }

                prevPrevObj = prevObj;
                prevObj = currObj;
            }

            // If the current island is long we don't want the sum to have as big of an effect
            rhythmComplexitySum *= DiffUtils.ReverseLerp(island.DeltaCount, 22, 3);

            return Math.Sqrt(4 + rhythmComplexitySum * rhythm_overall_multiplier) / 2.0; // produces multiplier that can be applied to strain. range [1, infinity) (not really though);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double getEffectiveRatio(double deltaDifference)
        {
            // Take only the fractional part of the value since we're only interested in punishing multiples
            double deltaDifferenceFraction = deltaDifference - Math.Truncate(deltaDifference);

            return 1.0 + rhythm_ratio_multiplier * Math.Min(0.5, DiffUtils.SmoothstepBellCurve(deltaDifferenceFraction));
        }

        /// <summary>
        /// An island is a thing. I'm not sure what thing it is, but it's definitely a thing.
        /// TODO: document this stuff please.
        /// </summary>
        private class Island
        {
            public int Delta { get; private set; }
            public int DeltaCount { get; private set; } = 1;

            public Island(int delta)
            {
                Delta = Math.Max(delta, OsuDifficultyHitObject.MIN_DELTA_TIME);
            }

            public void AddDelta(int delta)
            {
                if (Delta == int.MaxValue)
                    Delta = Math.Max(delta, OsuDifficultyHitObject.MIN_DELTA_TIME);

                DeltaCount++;
            }

            public bool IsSimilarPolarity(Island other, double epsilon)
            {
                // single delta islands shouldn't be compared
                if (DeltaCount <= 1 || other.DeltaCount <= 1)
                    return false;

                return Math.Abs(Delta - other.Delta) < epsilon &&
                       DeltaCount % 2 == other.DeltaCount % 2;
            }

            public bool AlmostEquals(Island other, double epsilon)
            {
                return Math.Abs(Delta - other.Delta) < epsilon &&
                       DeltaCount == other.DeltaCount;
            }

            public override string ToString()
            {
                return $"{Delta}x{DeltaCount}";
            }
        }
    }
}
