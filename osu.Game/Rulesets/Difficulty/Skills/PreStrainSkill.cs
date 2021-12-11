using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class PreStrainSkill : StrainDecaySkill
    {
        public PreStrainSkill(Mod[] mods) : base(mods)
        {
        }

        public List<double> GetAllStrainPeaks() => strainPeaks;

        protected override void Process(int index, DifficultyHitObject current)
        {
            currentSectionPeak *= StrainDecayBase;
            currentSectionPeak += StrainValueAt(index, current);
            saveCurrentPeak();
        }

        public double this[int i] => strainPeaks[i];
    }
}
