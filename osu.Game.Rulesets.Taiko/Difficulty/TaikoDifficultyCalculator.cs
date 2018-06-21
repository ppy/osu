// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    internal class TaikoDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.04125;

        /// <summary>
        /// In milliseconds. For difficulty calculation we will only look at the highest strain value in each time interval of size STRAIN_STEP.
        /// This is to eliminate higher influence of stream over aim by simply having more HitObjects with high strain.
        /// The higher this value, the less strains there will be, indirectly giving long beatmaps an advantage.
        /// </summary>
        private const double strain_step = 400;

        /// <summary>
        /// The weighting of each strain value decays to this number * it's previous value
        /// </summary>
        private const double decay_weight = 0.9;

        public TaikoDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double timeRate)
        {
            if (!beatmap.HitObjects.Any())
                return new TaikoDifficultyAttributes(mods, 0);

            var difficultyHitObjects = new List<TaikoHitObjectDifficulty>();

            foreach (var hitObject in beatmap.HitObjects)
                difficultyHitObjects.Add(new TaikoHitObjectDifficulty((TaikoHitObject)hitObject));

            // Sort DifficultyHitObjects by StartTime of the HitObjects - just to make sure.
            difficultyHitObjects.Sort((a, b) => a.BaseHitObject.StartTime.CompareTo(b.BaseHitObject.StartTime));

            if (!calculateStrainValues(difficultyHitObjects, timeRate))
                return new DifficultyAttributes(mods, 0);

            double starRating = calculateDifficulty(difficultyHitObjects, timeRate) * star_scaling_factor;

            return new TaikoDifficultyAttributes(mods, starRating)
            {
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be remoevd in the future
                GreatHitWindow = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / timeRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit)
            };
        }

        private bool calculateStrainValues(List<TaikoHitObjectDifficulty> objects, double timeRate)
        {
            // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
            using (var hitObjectsEnumerator = objects.GetEnumerator())
            {
                if (!hitObjectsEnumerator.MoveNext()) return false;

                TaikoHitObjectDifficulty current = hitObjectsEnumerator.Current;

                // First hitObject starts at strain 1. 1 is the default for strain values, so we don't need to set it here. See DifficultyHitObject.
                while (hitObjectsEnumerator.MoveNext())
                {
                    var next = hitObjectsEnumerator.Current;
                    next?.CalculateStrains(current, timeRate);
                    current = next;
                }

                return true;
            }
        }

        private double calculateDifficulty(List<TaikoHitObjectDifficulty> objects, double timeRate)
        {
            double actualStrainStep = strain_step * timeRate;

            // Find the highest strain value within each strain step
            List<double> highestStrains = new List<double>();
            double intervalEndTime = actualStrainStep;
            double maximumStrain = 0; // We need to keep track of the maximum strain in the current interval

            TaikoHitObjectDifficulty previousHitObject = null;
            foreach (var hitObject in objects)
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
                        double decay = Math.Pow(TaikoHitObjectDifficulty.DECAY_BASE, (intervalEndTime - previousHitObject.BaseHitObject.StartTime) / 1000);
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

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };
    }
}
