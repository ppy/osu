// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuStrainSkill : StrainSkill
    {
        protected OsuStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p > 0);

            List<double> hardStrains = new List<double>();

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in peaks.OrderDescending())
            {
                // We are taking only the spikes that fit in the 80% of the top weight spike in strains to achieve a good perception of the map's peak sections.
                if (strain / peaks.Max() > 0.8)
                    hardStrains.Add(strain);

                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            // We can calculate the consitency factor by doing average of spikes / most weight spikes.
            // It result in a value that represent the consistency for all peaks in a range number from 0 to 1.
            ConsistencyFactor = peaks.Average() / hardStrains.Average();

            return difficulty;
        }
        public static double DifficultyToPerformance(double difficulty) => Math.Pow(5.0 * Math.Max(1.0, difficulty / 0.0675) - 4.0, 3.0) / 100000.0;
    }
}
