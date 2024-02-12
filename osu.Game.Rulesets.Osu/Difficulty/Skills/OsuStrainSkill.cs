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

        /// <summary>
        /// The bonus multiplier that is given for a sequence of notes of equal difficulty.
        /// </summary>
        protected virtual double StarsPerDouble => 1.05;

        protected OsuStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p > 0);

            List<double> strains = peaks.ToList();

            if (ReducedSectionCount > 0)
            {
                strains = strains.OrderDescending().ToList();

                // We are reducing the highest strains first to account for extreme difficulty spikes
                for (int i = 0; i < Math.Min(strains.Count, ReducedSectionCount); i++)
                {
                    double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((float)i / ReducedSectionCount, 0, 1)));
                    strains[i] *= Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale);
                }
            }

            // Math here preserves the property that two notes of equal difficulty x, we have their summed difficulty = x * starsPerDouble.
            // This also applies to two sets of notes with equal difficulty.
            foreach (double strain in strains)
                difficulty += Math.Pow(strain, 1 / Math.Log(StarsPerDouble, 2));

            return Math.Pow(difficulty, Math.Log(StarsPerDouble, 2));
        }
    }
}
