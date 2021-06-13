using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuSkill : StrainSkill
    {
        public OsuSkill(Mod[] mods) : base(mods)
        {
        }

        public double OsuDifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            double strainMultiplier;
            List<double> strains = GetCurrentStrainPeaks().OrderByDescending(d => d).ToList();

            double baseLine = 0.68;

            for (int i = 0; i <= 9; i++)
            {
                strainMultiplier = baseLine + Math.Log10(i+1) * (1.0 - baseLine);
                strains[i] = strains[i] * strainMultiplier;
            }

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in strains.OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty * 1.06;
        }
    }
}
