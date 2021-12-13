// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        protected override void Process(DifficultyHitObject current)
        {
            CurrentSectionPeak *= StrainDecayBase;
            CurrentSectionPeak += StrainValueAt(current);
            SaveCurrentPeak();
        }

        public double GetCurrentStrain() => StrainPeaks[StrainPeaks.Count - 1];

        public double GetLastStrain() => StrainPeaks[StrainPeaks.Count - 2];

        public double this[int i] => StrainPeaks[StrainPeaks.Count - 1 - i];

        public override double DifficultyValue()
        {
            return 0;
        }

        protected override double CalculateInitialStrain(double time) => 0.0;
    }
}
