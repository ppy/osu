// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuVariableLengthStrainSkill : VariableLengthStrainSkill
    {
        protected double RawDifficultyMultiplier => 1.01;

        /// <summary>
        /// The number of sections with the highest strains, which the peak strain reductions will apply to.
        /// This is done in order to decrease their impact on the overall difficulty of the map for this skill.
        /// </summary>
        protected virtual int ReducedSectionCount => 10;

        /// <summary>
        /// The baseline multiplier applied to the section with the biggest strain.
        /// </summary>
        protected virtual double ReducedStrainBaseline => 0.75;

        protected OsuVariableLengthStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p.Value > 0);

            List<Strain> strains = peaks.OrderByDescending(p => (p.Value, p.SectionLength)).ToList();

            // Time is measured in units of strains
            double time = 0;

            // We are reducing the highest strains first to account for extreme difficulty spikes
            for (int i = 0; i < strains.Count && time < ReducedSectionCount; i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((float)time / ReducedSectionCount, 0, 1)));
                strains[i] = new Strain(strains[i].Value * Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale), strains[i].SectionLength);
                time += strains[i].SectionLength / MaxSectionLength;
            }

            strains = strains.OrderByDescending(s => s.Value).ToList();
            time = 0;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            for (int i = 0; i < strains.Count; i++)
            {
                difficulty += strains[i].Value * weight * strains[i].SectionLength / MaxSectionLength;
                time += strains[i].SectionLength / MaxSectionLength;
                weight = Math.Pow(DecayWeight, time);
            }

            return difficulty * RawDifficultyMultiplier;
        }

        public static double DifficultyToPerformance(double difficulty) => Math.Pow(5.0 * Math.Max(1.0, difficulty / 0.0675) - 4.0, 3.0) / 100000.0;
    }
}
