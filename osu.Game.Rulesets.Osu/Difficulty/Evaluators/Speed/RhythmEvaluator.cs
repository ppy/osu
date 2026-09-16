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
        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            const int history_time_max = 5 * 1000; // 5 seconds
            const int history_objects_max = 32;
            const double rhythm_overall_multiplier = 0.95;

            double rhythmComplexitySum = 0;

            double deltaDifferenceEpsilon = ((OsuDifficultyHitObject)current).HitWindowGreat * 0.3;

            var island = new Island(int.MaxValue);
            var previousIsland = new Island(int.MaxValue);

            var islands = new List<Island>();

            double startDifficulty = 0; // store the difficulty of the current start of an island to buff for tighter rhythms

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

                double deltaDifference = Math.Abs(prevDelta - currDelta);

                // Make sure to always have the current island initialised - if we don't do it here it will only initialise on the next rhythm change
                if (island.Delta == int.MaxValue)
                    island = new Island((int)currDelta);

                // calculate how much current delta difference deserves a rhythm bonus
                // this function is meant to reduce rhythm bonus for deltas that are multiples of each other (i.e 100 and 200)
                double deltaDifferenceRatio = Math.Max(prevDelta, currDelta) / Math.Min(prevDelta, currDelta);

                // reduce ratio bonus if delta difference is too big
                double differenceMultiplier = Math.Clamp(2.0 - deltaDifferenceRatio / 8.0, 0.0, 1.0);

                double windowPenalty = Math.Clamp((deltaDifference - deltaDifferenceEpsilon) / deltaDifferenceEpsilon, 0, 1);

                double effectiveDifficulty = getEffectiveDifficulty(deltaDifferenceRatio) * windowPenalty * differenceMultiplier;

                // if previous object is a slider it might be easier to tap since you don't have to do a whole tapping motion
                // while a full deltatime might end up some weird ratio the "unpress->tap" motion might be simple
                // for example a slider-circle-circle pattern should be evaluated as a regular triple and not as a single->double
                if (prevObj.BaseObject is Slider)
                {
                    double sliderLazyEndDelta = currObj.MinimumJumpTime;
                    double sliderLazyDeltaDifferenceRatio = Math.Max(sliderLazyEndDelta, currDelta) / Math.Min(sliderLazyEndDelta, currDelta);

                    double sliderRealEndDelta = currObj.LastObjectEndDeltaTime;
                    double sliderRealDeltaDifferenceRatio = Math.Max(sliderRealEndDelta, currDelta) / Math.Min(sliderRealEndDelta, currDelta);

                    double sliderEffectiveDifficulty = Math.Min(getEffectiveDifficulty(sliderLazyDeltaDifferenceRatio), getEffectiveDifficulty(sliderRealDeltaDifferenceRatio));
                    effectiveDifficulty = Math.Min(sliderEffectiveDifficulty, effectiveDifficulty);
                }

                if (deltaDifference < deltaDifferenceEpsilon)
                {
                    // island is still progressing
                    island.AddDelta((int)currDelta);
                }

                if (firstDeltaSwitch)
                {
                    if (deltaDifference > deltaDifferenceEpsilon)
                    {
                        // bpm change is into slider, this is easy acc window
                        if (currObj.BaseObject is Slider)
                            effectiveDifficulty *= 0.5;

                        // repeated island polarity (2 -> 4, 3 -> 5)
                        if (island.IsSimilarPolarity(previousIsland, deltaDifferenceEpsilon))
                            effectiveDifficulty *= 0.5;

                        // previous increase happened a note ago, 1/1->1/2-1/4, dont want to buff this.
                        if (Math.Max(prevPrevObj.DeltaTime, delta_min_value) > prevDelta + deltaDifferenceEpsilon && prevDelta > currDelta + deltaDifferenceEpsilon)
                            effectiveDifficulty *= 0.125;

                        // repeated island size (ex: triplet -> triplet)
                        // TODO: remove this nerf since its staying here only for balancing purposes because of the flawed ratio calculation
                        if (previousIsland.DeltaCount == island.DeltaCount)
                            effectiveDifficulty *= 0.5;

                        bool isSpeedingUp = prevDelta > currDelta + deltaDifferenceEpsilon;

                        if (isSpeedingUp)
                            effectiveDifficulty *= 0.65;

                        bool found = false;

                        foreach (var existingIsland in islands)
                        {
                            if (existingIsland.AlmostEquals(island, deltaDifferenceEpsilon))
                            {
                                // only increase island occurrences if they're going one after another
                                if (previousIsland.AlmostEquals(island, deltaDifferenceEpsilon))
                                    existingIsland.Occurrences++;

                                // repeated island (ex: triplet -> triplet)
                                double power = DiffUtils.Logistic(island.Delta, maxValue: 2.75, multiplier: 0.24, midpointOffset: 58.33);
                                effectiveDifficulty *= Math.Min(3.0 / existingIsland.Occurrences, DiffUtils.Pow(1.0 / existingIsland.Occurrences, power));

                                found = true;
                                break;
                            }
                        }

                        if (!found && island.DeltaCount > 0)
                            islands.Add(island);

                        // scale down the difficulty if the object is double-tappable
                        effectiveDifficulty *= 1 - prevObj.CalculateDoubleTapFeasibility(currObj) * 0.75;

                        if (island.DeltaCount > 1)
                        {
                            rhythmComplexitySum += Math.Sqrt(effectiveDifficulty * startDifficulty) * currHistoricalDecay;
                        }
                        else
                        {
                            // constant difficulty for single-note islands
                            rhythmComplexitySum += 0.7 * currHistoricalDecay;
                        }

                        startDifficulty = effectiveDifficulty;

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
                        effectiveDifficulty *= 0.6;

                    // bpm change was from a slider, this is easier typically than circle -> circle
                    // unintentional side effect is that bursts with kicksliders at the ends might have lower difficulty than bursts without sliders
                    if (prevObj.BaseObject is Slider)
                        effectiveDifficulty *= 0.6;

                    startDifficulty = effectiveDifficulty;

                    island = new Island((int)currDelta);
                }

                prevPrevObj = prevObj;
                prevObj = currObj;
            }

            // If the current island is long we don't want the sum to have as big of an effect
            rhythmComplexitySum *= DiffUtils.ReverseLerp(island.DeltaCount, 22, 3);

            return Math.Sqrt(4 + rhythmComplexitySum * rhythm_overall_multiplier) / 2.0; // produces multiplier that can be applied to strain. range [1, infinity) (not really though)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double getEffectiveDifficulty(double deltaDifferenceRatio)
        {
            const double rhythm_ratio_difficulty_multiplier = 26.0;

            // Take only the fractional part of the value since we're only interested in punishing multiples
            double deltaDifferenceFraction = deltaDifferenceRatio - Math.Truncate(deltaDifferenceRatio);

            return 1.0 + rhythm_ratio_difficulty_multiplier * Math.Min(0.5, DiffUtils.SmoothstepBellCurve(deltaDifferenceFraction));
        }

        /// <summary>
        /// An island is a group of consecutive objects with the same delta time.
        /// </summary>
        private class Island
        {
            /// <summary>
            /// Delta time of every object in this island
            /// </summary>
            public int Delta { get; private set; }

            /// <summary>
            /// How long the island is
            /// </summary>
            public int DeltaCount { get; private set; } = 1;

            /// <summary>
            /// How many times island already occured
            /// </summary>
            public int Occurrences { get; set; } = 1;

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
