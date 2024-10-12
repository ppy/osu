// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Framework.Utils;
using osu.Framework.Lists;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuStrainSkill : StrainSkill
    {
        /// <summary>
        /// The number of sections with the highest strains, which the peak strain reductions will apply to.
        /// This is done in order to decrease their impact on the overall difficulty of the map for this skill.
        /// </summary>
        protected virtual int ReducedSectionCount => 10;

        /// <summary>
        /// The baseline multiplier applied to the section with the biggest strain.
        /// </summary>
        protected virtual double ReducedStrainBaseline => 0.75;

        protected OsuStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            SortedList<double> strains = GetCurrentStrainsSorted();

            int reducedSectionCount = Math.Min(strains.Count, ReducedSectionCount);
            double[] reducedStrains = new double[reducedSectionCount];

            // We are reducing the highest strains first to account for extreme difficulty spikes
            for (int i = 0; i < reducedSectionCount; i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((float)i / ReducedSectionCount, 0, 1)));
                reducedStrains[i] = strains[i] * Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale);
            }

            // Remove reduced strains as they are no longer sorted
            strains.RemoveRange(0, reducedSectionCount);

            // Insert them back
            strains.AddRange(reducedStrains);

            // Difficulty is the weighted sum of the highest strains from every section.
            foreach (double strain in strains)
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty;
        }

        public static double DifficultyToPerformance(double difficulty) => Math.Pow(5.0 * Math.Max(1.0, difficulty / 0.0675) - 4.0, 3.0) / 100000.0;
    }
}
