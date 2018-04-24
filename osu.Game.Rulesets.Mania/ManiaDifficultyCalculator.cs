// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania
{
    internal class ManiaDifficultyCalculator : DifficultyCalculator<ManiaHitObject>
    {
        private const double star_scaling_factor = 0.018;

        /// <summary>
        /// In milliseconds. For difficulty calculation we will only look at the highest strain value in each time interval of size strain_step.
        /// This is to eliminate higher influence of stream over aim by simply having more HitObjects with high strain.
        /// The higher this value, the less strains there will be, indirectly giving long beatmaps an advantage.
        /// </summary>
        private const double strain_step = 400;

        /// <summary>
        /// The weighting of each strain value decays to this number * it's previous value
        /// </summary>
        private const double decay_weight = 0.9;

        /// <summary>
        /// HitObjects are stored as a member variable.
        /// </summary>
        private readonly List<ManiaHitObjectDifficulty> difficultyHitObjects = new List<ManiaHitObjectDifficulty>();

        public ManiaDifficultyCalculator(Beatmap beatmap)
            : base(beatmap)
        {
        }

        public ManiaDifficultyCalculator(Beatmap beatmap, Mod[] mods)
            : base(beatmap, mods)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            // Fill our custom DifficultyHitObject class, that carries additional information
            difficultyHitObjects.Clear();

            int columnCount = (Beatmap as ManiaBeatmap)?.TotalColumns ?? 7;

            foreach (var hitObject in Beatmap.HitObjects)
                difficultyHitObjects.Add(new ManiaHitObjectDifficulty(hitObject, columnCount));

            // Sort DifficultyHitObjects by StartTime of the HitObjects - just to make sure.
            difficultyHitObjects.Sort((a, b) => a.BaseHitObject.StartTime.CompareTo(b.BaseHitObject.StartTime));

            if (!calculateStrainValues())
                return 0;

            double starRating = calculateDifficulty() * star_scaling_factor;

            categoryDifficulty?.Add("Strain", starRating);

            return starRating;
        }

        private bool calculateStrainValues()
        {
            // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
            using (List<ManiaHitObjectDifficulty>.Enumerator hitObjectsEnumerator = difficultyHitObjects.GetEnumerator())
            {
                if (!hitObjectsEnumerator.MoveNext())
                    return false;

                ManiaHitObjectDifficulty current = hitObjectsEnumerator.Current;

                // First hitObject starts at strain 1. 1 is the default for strain values, so we don't need to set it here. See DifficultyHitObject.
                while (hitObjectsEnumerator.MoveNext())
                {
                    var next = hitObjectsEnumerator.Current;
                    next?.CalculateStrains(current, TimeRate);
                    current = next;
                }

                return true;
            }
        }

        private double calculateDifficulty()
        {
            double actualStrainStep = strain_step * TimeRate;

            // Find the highest strain value within each strain step
            List<double> highestStrains = new List<double>();
            double intervalEndTime = actualStrainStep;
            double maximumStrain = 0; // We need to keep track of the maximum strain in the current interval

            ManiaHitObjectDifficulty previousHitObject = null;
            foreach (var hitObject in difficultyHitObjects)
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
                        double individualDecay = Math.Pow(ManiaHitObjectDifficulty.INDIVIDUAL_DECAY_BASE, (intervalEndTime - previousHitObject.BaseHitObject.StartTime) / 1000);
                        double overallDecay = Math.Pow(ManiaHitObjectDifficulty.OVERALL_DECAY_BASE, (intervalEndTime - previousHitObject.BaseHitObject.StartTime) / 1000);
                        maximumStrain = previousHitObject.IndividualStrain * individualDecay + previousHitObject.OverallStrain * overallDecay;
                    }

                    // Go to the next time interval
                    intervalEndTime += actualStrainStep;
                }

                // Obtain maximum strain
                double strain = hitObject.IndividualStrain + hitObject.OverallStrain;
                maximumStrain = Math.Max(strain, maximumStrain);

                previousHitObject = hitObject;
            }

            // Build the weighted sum over the highest strains for each interval
            double difficulty = 0;
            double weight = 1;

            // Averages the map strain, can be used instead of the moving average but needs commenting some code
            //double averageStrain = highestStrains.Average(strain => strain);

            Queue<double> currentStrains = new Queue<double>();

            int memory = 150; // number * 400ms = seconds memory | Here, a minute

            // Stability reward system
            double overweightFactor = 1.4; // When should we consider that stability is dropping because of higher strain
            double underweightFactor = 0.7; // or lower strain

            double stability = 1; // Stability base

            double maxStability = 1.12; // Maximum difficulty bonus for stable difficulty
            double minStability = 0.9;  // Maximum difficulty malus for unstable difficulty

            double stabilityStepDecrease = 0.04; // Punishment growth rate for unstable parts
            double stabilityStepIncrease = 0.01; // Reward growth rate for stable parts


            List<double> factors = new List<double>();

            // Weight strains according to the stability to avoid burst overrating
            foreach (double strain in highestStrains)
            {
                // Use a queue tu keep "memory" seconds of strain
                currentStrains.Enqueue(strain);
                if (currentStrains.Count > memory)
                {
                    currentStrains.Dequeue();
                }
                // Average those
                double averageStrain = currentStrains.Average();

                double factor = strain / averageStrain;

                // Move the stability according to our constants
                if (factor > overweightFactor)
                {
                    stability -= stabilityStepDecrease;
                }
                else
                if (factor < underweightFactor)
                {
                    stability -= stabilityStepDecrease;
                }
                else
                {
                    stability += stabilityStepIncrease;
                }

                // The stability is capped
                if (stability > maxStability) { stability = maxStability; }
                if (stability < minStability) { stability = minStability; }

                // Add difficulty according to strain and stability
                factors.Add(stability);
            }

            // Apply stability to each strains - CAN be improved I'm sure
            // The best would be applying it while going though the previous block
            int index = 0;
            foreach (double stabilityFactor in factors)
            {
                highestStrains[index] *= stabilityFactor;
                index++;
            }

            highestStrains.Sort((a, b) => b.CompareTo(a)); // Sort from highest to lowest strain.

            // Weighted sum
            foreach (double strain in highestStrains)
            {
                difficulty += weight * strain;
                weight *= decay_weight;
            }


            return difficulty;
        }

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter(Beatmap beatmap) => new ManiaBeatmapConverter(true, beatmap);
    }
}
