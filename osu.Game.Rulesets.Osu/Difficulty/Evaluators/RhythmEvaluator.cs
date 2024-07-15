// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        private readonly struct Island : IEquatable<Island>
        {
            public Island()
            {
            }

            public Island(int firstDelta, double epsilon)
            {
                AddDelta(firstDelta, epsilon);
            }

            public List<int> Deltas { get; } = new List<int>();

            public void AddDelta(int delta, double epsilon)
            {
                int existingDelta = Deltas.FirstOrDefault(x => Math.Abs(x - delta) >= epsilon);

                Deltas.Add(existingDelta == default ? delta : existingDelta);
            }

            public double AverageDelta() => Math.Max(Deltas.Average(), OsuDifficultyHitObject.MIN_DELTA_TIME);

            public override int GetHashCode()
            {
                // we need to compare all deltas and they must be in the exact same order we added them
                string joinedDeltas = string.Join(string.Empty, Deltas);
                return joinedDeltas.GetHashCode();
            }

            public bool Equals(Island other)
            {
                return other.GetHashCode() == GetHashCode();
            }

            public override bool Equals(object? obj)
            {
                return obj?.GetHashCode() == GetHashCode();
            }
        }

        private const int history_time_max = 5 * 1000; // 5 seconds of calculatingRhythmBonus max.
        private const double rhythm_multiplier = 1.14;
        private const int max_island_size = 7;

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            Dictionary<Island, int> islandCounts = new Dictionary<Island, int>();

            if (current.BaseObject is Spinner)
                return 0;

            double rhythmComplexitySum = 0;

            var island = new Island();
            var previousIsland = new Island();

            double startRatio = 0; // store the ratio of the current start of an island to buff for tighter rhythms

            bool firstDeltaSwitch = false;

            int historicalNoteCount = Math.Min(current.Index, 32);

            int rhythmStart = 0;

            while (rhythmStart < historicalNoteCount - 2 && current.StartTime - current.Previous(rhythmStart).StartTime < history_time_max)
                rhythmStart++;

            OsuDifficultyHitObject prevObj = (OsuDifficultyHitObject)current.Previous(rhythmStart);
            OsuDifficultyHitObject lastObj = (OsuDifficultyHitObject)current.Previous(rhythmStart + 1);

            for (int i = rhythmStart; i > 0; i--)
            {
                OsuDifficultyHitObject currObj = (OsuDifficultyHitObject)current.Previous(i - 1);

                double currHistoricalDecay = (history_time_max - (current.StartTime - currObj.StartTime)) / history_time_max; // scales note 0 to 1 from history to now

                currHistoricalDecay = Math.Min((double)(historicalNoteCount - i) / historicalNoteCount, currHistoricalDecay); // either we're limited by time or limited by object count.

                double currDelta = currObj.StrainTime;
                double prevDelta = prevObj.StrainTime;
                double lastDelta = lastObj.StrainTime;

                double currRatio = 1.0 + 6.0 * Math.Min(0.5, Math.Pow(Math.Sin(Math.PI / (Math.Min(prevDelta, currDelta) / Math.Max(prevDelta, currDelta))), 2)); // fancy function to calculate rhythmbonuses.

                double windowPenalty = Math.Min(1, Math.Max(0, Math.Abs(prevDelta - currDelta) - currObj.HitWindowGreat * 0.3) / (currObj.HitWindowGreat * 0.3));

                windowPenalty = Math.Min(1, windowPenalty);

                double effectiveRatio = windowPenalty * currRatio;

                double deltaDifferenceEpsilon = currObj.HitWindowGreat * 0.3;

                if (firstDeltaSwitch)
                {
                    if (!(Math.Abs(prevDelta - currDelta) > deltaDifferenceEpsilon))
                    {
                        if (island.Deltas.Count < max_island_size)
                        {
                            // island is still progressing
                            island.AddDelta((int)currDelta, deltaDifferenceEpsilon);
                        }
                    }
                    else
                    {
                        if (currObj.BaseObject is Slider) // bpm change is into slider, this is easy acc window
                            effectiveRatio *= 0.125;

                        if (prevObj.BaseObject is Slider) // bpm change was from a slider, this is easier typically than circle -> circle
                            effectiveRatio *= 0.25;

                        if (previousIsland.Deltas.Count % 2 == island.Deltas.Count % 2) // repeated island polartiy (2 -> 4, 3 -> 5)
                            effectiveRatio *= 0.50;

                        if (lastDelta > prevDelta + deltaDifferenceEpsilon && prevDelta > currDelta + deltaDifferenceEpsilon) // previous increase happened a note ago, 1/1->1/2-1/4, dont want to buff this.
                            effectiveRatio *= 0.125;

                        if (islandCounts.ContainsKey(island))
                        {
                            islandCounts[island]++;

                            // repeated island (ex: triplet -> triplet)
                            double power = Math.Max(0.75, logistic(island.AverageDelta(), 3, 0.15, 9));
                            effectiveRatio *= Math.Pow(1.0 / islandCounts[island], power);
                        }
                        else
                        {
                            islandCounts.Add(island, 1);
                        }

                        rhythmComplexitySum += Math.Sqrt(effectiveRatio * startRatio) * currHistoricalDecay;

                        startRatio = effectiveRatio;

                        previousIsland = island;

                        if (prevDelta + deltaDifferenceEpsilon < currDelta) // we're slowing down, stop counting
                            firstDeltaSwitch = false; // if we're speeding up, this stays true and  we keep counting island size.

                        island = new Island((int)currDelta, deltaDifferenceEpsilon);
                    }
                }
                else if (prevDelta > currDelta + deltaDifferenceEpsilon) // we want to be speeding up.
                {
                    // Begin counting island until we change speed again.
                    firstDeltaSwitch = true;
                    startRatio = effectiveRatio;

                    island = new Island((int)currDelta, deltaDifferenceEpsilon);
                }

                lastObj = prevObj;
                prevObj = currObj;
            }

            return Math.Sqrt(4 + rhythmComplexitySum * rhythm_multiplier) / 2; //produces multiplier that can be applied to strain. range [1, infinity) (not really though)
        }

        private static double logistic(double x, double maxValue, double multiplier, double offset) => (maxValue / (1 + Math.Pow(Math.E, offset - multiplier * x)));
    }
}
