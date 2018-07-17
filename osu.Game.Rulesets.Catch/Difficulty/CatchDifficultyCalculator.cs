// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {

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

        private const double star_scaling_factor = 0.145;

        public CatchDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double timeRate)
        {
            if (!beatmap.HitObjects.Any())
                return new CatchDifficultyAttributes(mods, 0);

            var catcher = new CatcherArea.Catcher(beatmap.BeatmapInfo.BaseDifficulty);
            float halfCatchWidth = catcher.CatchWidth * 0.5f;

            var difficultyHitObjects = new List<CatchDifficultyHitObject>();

            foreach (var hitObject in beatmap.HitObjects)
            {
                // We want to only consider fruits that contribute to the combo. Droplets are addressed as accuracy and spinners are not relevant for "skill" calculations.
                if (hitObject is Fruit)
                {
                    difficultyHitObjects.Add(new CatchDifficultyHitObject((CatchHitObject)hitObject, halfCatchWidth));
                }
                if (hitObject is JuiceStream)
                    difficultyHitObjects.AddRange(hitObject.NestedHitObjects.OfType<CatchHitObject>().Where(o => !(o is TinyDroplet)).Select(o => new CatchDifficultyHitObject(o, halfCatchWidth)));
            }

            difficultyHitObjects.Sort((a, b) => a.BaseHitObject.StartTime.CompareTo(b.BaseHitObject.StartTime));

            if (!calculateStrainValues(difficultyHitObjects, timeRate))
                return new CatchDifficultyAttributes(mods, 0);

            // this is the same as osu!, so there's potential to share the implementation... maybe
            double preempt = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / timeRate;
            double starRating = Math.Sqrt(calculateDifficulty(difficultyHitObjects, timeRate)) * star_scaling_factor;

            return new CatchDifficultyAttributes(mods, starRating)
            {
                ApproachRate = preempt > 1200.0 ? -(preempt - 1800.0) / 120.0 : -(preempt - 1200.0) / 150.0 + 5.0,
                MaxCombo = difficultyHitObjects.Count
            };
        }

        private bool calculateStrainValues(List<CatchDifficultyHitObject> objects, double timeRate)
        {
            CatchDifficultyHitObject lastObject = null;

            if (!objects.Any()) return false;

            // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
            foreach (var currentObject in objects)
            {
                if (lastObject != null)
                    currentObject.CalculateStrains(lastObject, timeRate);

                lastObject = currentObject;
            }

            return true;
        }

        private double calculateDifficulty(List<CatchDifficultyHitObject> objects, double timeRate)
        {
            // The strain step needs to be adjusted for the algorithm to be considered equal with speed changing mods
            double actualStrainStep = strain_step * timeRate;

            // Find the highest strain value within each strain step
            var highestStrains = new List<double>();
            double intervalEndTime = actualStrainStep;
            double maximumStrain = 0; // We need to keep track of the maximum strain in the current interval

            CatchDifficultyHitObject previousHitObject = null;
            foreach (CatchDifficultyHitObject hitObject in objects)
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

            // Build the weighted sum over the highest strains for each interval
            double difficulty = 0;
            double weight = 1;
            highestStrains.Sort((a, b) => b.CompareTo(a)); // Sort from highest to lowest strain.

            foreach (double strain in highestStrains)
            {
                difficulty += weight * strain;
                weight *= decay_weight;
            }

            return difficulty;
        }
    }
}
