// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.MathUtil
{
    public class HitProbabilities
    {
        private readonly List<MapSectionCache> sections = new List<MapSectionCache>();

        public HitProbabilities(List<OsuMovement> movements, double cheeseLevel, int difficultyCount = 20)
        {
            for (int i = 0; i < difficultyCount; ++i)
            {
                int start = movements.Count * i / difficultyCount;
                int end = movements.Count * (i + 1) / difficultyCount - 1;
                double startT = movements[start].Time;
                double endT = movements[end].Time;
                sections.Add(new MapSectionCache(movements.GetRange(start, end - start + 1), cheeseLevel, startT, endT));
            }
        }

        public static double CalculateCheeseHitProb(OsuMovement movement, double tp, double cheeseLevel)
        {
            double perMovementCheeseLevel = cheeseLevel;

            if (movement.EndsOnSlider)
                perMovementCheeseLevel = 0.5 * cheeseLevel + 0.5;

            double cheeseMt = movement.Mt * (1 + perMovementCheeseLevel * movement.CheesableRatio);
            return FittsLaw.CalculateHitProb(movement.D, cheeseMt, tp);
        }

        public double FcProbability(double tp)
        {
            double fcProb = 1;

            foreach (var section in sections)
            {
                fcProb *= section.Evaluate(tp).FcProbability;
            }

            return fcProb;
        }

        /// <summary>
        /// Calculates (expected time for FC - duration of the submap) for every submap that spans sectionCount sections
        /// and takes the minimum value.
        /// </summary>
        public double MinExpectedTimeForSectionCount(double tp, int sectionCount)
        {
            double fcTime = double.PositiveInfinity;

            var sectionData = sections.Select(x => x.Evaluate(tp)).ToArray();

            for (int i = 0; i <= sections.Count - sectionCount; i++)
            {
                fcTime = Math.Min(fcTime, expectedFcTime(sectionData, i, sectionCount) - Length(i, sectionCount));
            }

            return fcTime;
        }

        /// <summary>
        /// Calculates duration of the submap
        /// </summary>
        public double Length(int start, int sectionCount)
        {
            return sections[start + sectionCount - 1].EndT - sections[start].StartT;
        }


        /// <summary>
        /// Average time it takes a player to FC this subset assuming they retry as soon as they miss a note
        /// </summary>
        private static double expectedFcTime(SkillData[] sectionData, int start, int count)
        {
            double fcTime = 15;

            for (int i = start; i < start + count; ++i)
            {
                fcTime /= sectionData[i].FcProbability;
                fcTime += sectionData[i].ExpectedTime;
            }

            return fcTime;
        }

        private struct SkillData
        {
            public double ExpectedTime;
            public double FcProbability;
        }

        private class MapSectionCache
        {
            private readonly Dictionary<double, SkillData> cache = new Dictionary<double, SkillData>();
            private readonly double cheeseLevel;

            public readonly double StartT;
            public readonly double EndT;

            public List<OsuMovement> Movements { get; }

            public MapSectionCache(List<OsuMovement> movements, double cheeseLevel, double startT, double endT)
            {
                Movements = movements;
                StartT = startT;
                EndT = endT;

                this.cheeseLevel = cheeseLevel;
            }

            public SkillData Evaluate(double tp)
            {
                if (Movements.Count == 0)
                    return new SkillData { ExpectedTime = 0, FcProbability = 1 };

                if (cache.TryGetValue(tp, out SkillData result))
                {
                    return result;
                }

                result.ExpectedTime = 0;
                result.FcProbability = 1;

                foreach (OsuMovement movement in Movements)
                {
                    double hitProb = CalculateCheeseHitProb(movement, tp, cheeseLevel) + 1e-10;

                    // This line nerfs notes with high miss probability
                    hitProb = 1 - (Math.Sqrt(1 - hitProb + 0.25) - 0.5);

                    result.ExpectedTime = (result.ExpectedTime + movement.RawMt) / hitProb;
                    result.FcProbability *= hitProb;
                }

                cache.Add(tp, result);

                return result;
            }
        }
    }
}
