using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuStrainSkill : StrainSkill
    {
        protected virtual int ReducedSectionCount => 9;
        protected virtual double ReducedStrainBaseline => 0.68;
        protected virtual double DifficultyMultiplier => 1.06;

        public OsuStrainSkill(Mod[] mods) : base(mods)
        {
        }

        public double OsuDifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            double strainMultiplier;
            List<double> strains = GetCurrentStrainPeaks().OrderByDescending(d => d).ToList();

            for (int i = 0; i < ReducedSectionCount; i++)
            {
                strainMultiplier = ReducedStrainBaseline + Math.Log10(i * 9.0 / ReducedSectionCount + 1) * (1.0 - ReducedStrainBaseline);
                strains[i] = strains[i] * strainMultiplier;
            }

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in strains.OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty * DifficultyMultiplier;
        }
    }
}
