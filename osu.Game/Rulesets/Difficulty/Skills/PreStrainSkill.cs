// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class PreStrainSkill : StrainSkill
    {
        protected abstract double StrainDecayBase { get; }

        protected abstract double SkillMultiplier { get; }

        protected PreStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public List<double> GetAllStrainPeaks() => StrainPeaks;

        protected override void Process(int index, DifficultyHitObject current)
        {
            CurrentSectionPeak *= StrainDecayBase;
            CurrentSectionPeak += StrainValueAt(index, current);
            saveCurrentPeak();
        }

        protected override double CalculateInitialStrain(double time) => 0;

        public void ProcessPre(int index, DifficultyHitObject current)
        {
            ProcessInternal(index, current);
        }

        public double this[int i] => StrainPeaks[i];
    }
}
