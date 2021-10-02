using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public class DecayingStrainSections
    {

        public DecayingValue strain;
        public StrainSections sections;

        public DecayingStrainSections(double strainDecayBase, int sectionLength=400)
        {
            strain = new DecayingValue(strainDecayBase);
            sections = new StrainSections(strain.ValueAtTime, sectionLength);
        }

        public double AddStrain(double time, double strainIncrease)
        {
            sections.UpdateTime(time);
            strain.IncrementValue(time, strainIncrease);
            sections.UpdateStrainPeak(strain.Value);
            return strain.Value;
        }
    }
}
