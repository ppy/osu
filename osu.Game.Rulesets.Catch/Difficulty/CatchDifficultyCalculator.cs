// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.145;
        private const float playfield_width = CatchPlayfield.BASE_WIDTH;

        private readonly List<CatchDifficultyHitObject> difficultyHitObjects = new List<CatchDifficultyHitObject>();

        public CatchDifficultyCalculator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public CatchDifficultyCalculator(IBeatmap beatmap, Mod[] mods)
            : base(beatmap, mods)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            difficultyHitObjects.Clear();

            float circleSize = Beatmap.BeatmapInfo.BaseDifficulty.CircleSize;
            float catcherWidth = (1.0f - 0.7f * (circleSize - 5) / 5) * 0.62064f * CatcherArea.CATCHER_SIZE;
            float catcherWidthHalf = catcherWidth / 2;
            catcherWidthHalf *= 0.8f;

            foreach (var hitObject in Beatmap.HitObjects)
            {
                // We want to only consider fruits that contribute to the combo. Droplets are addressed as accuracy and spinners are not relevant for "skill" calculations.
                if (hitObject is Fruit)
                {
                    difficultyHitObjects.Add(new CatchDifficultyHitObject((CatchHitObject)hitObject, catcherWidthHalf));
                }
                if (hitObject is JuiceStream)
                {
                    IEnumerator<HitObject> nestedHitObjectsEnumerator = hitObject.NestedHitObjects.GetEnumerator();
                    while (nestedHitObjectsEnumerator.MoveNext())
                    {
                        CatchHitObject objectInJuiceStream = (CatchHitObject)nestedHitObjectsEnumerator.Current;
                        if (!(objectInJuiceStream is TinyDroplet))
                            difficultyHitObjects.Add(new CatchDifficultyHitObject(objectInJuiceStream, catcherWidthHalf));
                    }
                    // Dispose the enumerator after counting all fruits.
                    nestedHitObjectsEnumerator.Dispose();
                }
            }

            difficultyHitObjects.Sort((a, b) => a.BaseHitObject.StartTime.CompareTo(b.BaseHitObject.StartTime));

            if (!CalculateStrainValues()) return 0;

            double starRating = Math.Sqrt(CalculateDifficulty()) * star_scaling_factor;

            if (categoryDifficulty != null)
            {
                categoryDifficulty["Aim"] = starRating;

                double ar = Beatmap.BeatmapInfo.BaseDifficulty.ApproachRate;
                double preEmpt = BeatmapDifficulty.DifficultyRange(ar, 1800, 1200, 450) / TimeRate;

                categoryDifficulty["AR"] = preEmpt > 1200.0 ? -(preEmpt - 1800.0) / 120.0 : -(preEmpt - 1200.0) / 150.0 + 5.0;
                categoryDifficulty["Max combo"] = difficultyHitObjects.Count;
            }

            return starRating;
        }

        protected bool CalculateStrainValues()
        {
            // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
            using (List<CatchDifficultyHitObject>.Enumerator hitObjectsEnumerator = difficultyHitObjects.GetEnumerator())
            {
                if (!hitObjectsEnumerator.MoveNext()) return false;

                CatchDifficultyHitObject currentHitObject = hitObjectsEnumerator.Current;

                // First hitObject starts at strain 1. 1 is the default for strain values, so we don't need to set it here. See DifficultyHitObject.
                while (hitObjectsEnumerator.MoveNext())
                {
                    CatchDifficultyHitObject nextHitObject = hitObjectsEnumerator.Current;
                    nextHitObject?.CalculateStrains(currentHitObject, TimeRate);
                    currentHitObject = nextHitObject;
                }

                return true;
            }
        }

        /// <summary>
        /// In milliseconds. For difficulty calculation we will only look at the highest strain value in each time interval of size STRAIN_STEP.
        /// This is to eliminate higher influence of stream over aim by simply having more HitObjects with high strain.
        /// The higher this value, the less strains there will be, indirectly giving long beatmaps an advantage.
        /// </summary>
        private const double strain_step = 750;

        /// <summary>
        /// The weighting of each strain value decays to this number * it's previous value
        /// </summary>
        private const double decay_weight = 0.94;

        protected double CalculateDifficulty()
        {
            // The strain step needs to be adjusted for the algorithm to be considered equal with speed changing mods
            double actualStrainStep = strain_step * TimeRate;

            // Find the highest strain value within each strain step
            List<double> highestStrains = new List<double>();
            double intervalEndTime = actualStrainStep;
            double maximumStrain = 0; // We need to keep track of the maximum strain in the current interval

            CatchDifficultyHitObject previousHitObject = null;
            foreach (CatchDifficultyHitObject hitObject in difficultyHitObjects)
            {
                // While we are beyond the current interval push the currently available maximum to our strain list
                while (hitObject.BaseHitObject.StartTime > intervalEndTime)
                {
                    highestStrains.Add(maximumStrain);

                    // The maximum strain of the next interval is not zero by default! We need to take the last hitObject we encountered, take its strain and apply the decay
                    // until the beginning of the next interval.
                    if (previousHitObject == null)
                    {
                        maximumStrain = 0;
                    }
                    else
                    {
                        double decay = Math.Pow(CatchDifficultyHitObject.DECAY_BASE, (intervalEndTime - previousHitObject.BaseHitObject.StartTime) / 1000);
                        maximumStrain = previousHitObject.Strain * decay;
                    }

                    // Go to the next time interval
                    intervalEndTime += actualStrainStep;
                }

                // Obtain maximum strain
                maximumStrain = Math.Max(hitObject.Strain, maximumStrain);

                previousHitObject = hitObject;
            }

            // calculate maximun strain difficulty
            double difficulty = StrainCalculator(highestStrains, decay_weight);

            return difficulty;
        }
    }
}
