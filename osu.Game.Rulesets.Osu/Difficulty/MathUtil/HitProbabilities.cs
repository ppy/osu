using System;
using System.Collections.Generic;
using System.Linq;

using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.MathUtil
{
    public class HitProbabilities
    {
        public static int cacheHit = 0;
        public static int cacheMiss = 0;

        private List<MapSectionCache> sections = new List<MapSectionCache>();

        public HitProbabilities(List<OsuMovement> movements, double cheeseLevel, int difficultyCount = 20)
        {
            for (int i = 0; i < difficultyCount; ++i)
            {
                int start = movements.Count * i / difficultyCount;
                int end = movements.Count * (i + 1) / difficultyCount - 1;
                sections.Add(new MapSectionCache(movements.GetRange(start, end - start + 1), cheeseLevel));
            }
        }

        public int Count(int start, int sectionCount)
        {
            int count = 0;
            for (int i = start; i != start + sectionCount; i++)
            {
                count += sections[i].Movements.Count;
            }

            return count;
        }

        public bool IsEmpty(int sectionCount)
        {
            bool isEmpty = false;
            for (int i = 0; i <= sections.Count - sectionCount; i++)
            {
                isEmpty = isEmpty || Count(i, sectionCount) == 0;
            }

            return isEmpty;
        }

        public double Length(int start, int sectionCount)
        {
            double first = 0, last = 0;
            for (int i = start; i != start + sectionCount; i++)
            {
                if (sections[i].Movements.Count != 0)
                {
                    first = sections[i].Movements.First().Time;
                    break;
                }
            }
            for (int i = start + sectionCount - 1; i != start - 1; i--)
            {
                if (sections[i].Movements.Count != 0)
                {
                    last = sections[i].Movements.Last().Time;
                    break;
                }
            }
            return last - first;
        }

        public double MinExpectedTimeForCount(double tp, int sectionCount)
        {
            double fcTime = double.PositiveInfinity;
            for (int i = 0; i <= sections.Count - sectionCount; i++)
            {
                fcTime = Math.Min(fcTime, ExpectedFcTime(tp, i, sectionCount) - Length(i, sectionCount));
            }

            return fcTime;
        }

        public double ExpectedFcTime(double tp, int start, int sectionCount)
        {
            double fcTime = 5;
            for (int i = start; i != start + sectionCount; i++)
            {
                if (sections[i].Movements.Count != 0)
                {
                    SkillData s = sections[i].Evaluate(tp);

                    fcTime /= s.FcProbability;
                    fcTime += s.ExpectedTime;
                }
            }
            return fcTime;
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

        public static double CalculateCheeseHitProb(OsuMovement movement, double tp, double cheeseLevel)
        {
            double perMovementCheeseLevel = cheeseLevel;

            if (movement.EndsOnSlider)
                perMovementCheeseLevel = 0.5 * cheeseLevel + 0.5;

            double cheeseMT = movement.MT * (1 + perMovementCheeseLevel * movement.CheesableRatio);
            return FittsLaw.CalculateHitProb(movement.D, cheeseMT, tp);
        }

        private struct SkillData
        {
            public double ExpectedTime;
            public double FcProbability;
        }


        private class MapSectionCache
        {
            private Dictionary<double, SkillData> cache = new Dictionary<double, SkillData>();
            private readonly double cheeseLevel;

            public List<OsuMovement> Movements { get; }

            public MapSectionCache(List<OsuMovement> movements, double cheeseLevel)
            {
                this.Movements = movements;
                this.cheeseLevel = cheeseLevel;
            }


            public SkillData Evaluate(double tp)
            {
                if (cache.TryGetValue(tp, out SkillData result))
                {
                    cacheHit++;
                    return result;
                }
                cacheMiss++;

                result.ExpectedTime = 0;
                result.FcProbability = 1;

                foreach (OsuMovement movement in Movements)
                {
                    double hitProb = CalculateCheeseHitProb(movement, tp, cheeseLevel)+1e-10;
                    result.ExpectedTime = (result.ExpectedTime + movement.RawMT) / hitProb;
                    result.FcProbability *= hitProb;
                }
                cache.Add(tp, result);

                return result;
            }
        }
    }
}
