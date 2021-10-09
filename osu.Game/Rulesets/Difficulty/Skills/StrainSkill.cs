// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Convenience base class for a<see cref="Skill"/> making use of <see cref="SectionPeaks"/>
    /// </summary>
    public abstract class StrainSkill : Skill
    {
        protected virtual double DifficultySumWeight => 0.9;

        protected readonly SectionPeaks StrainPeaks;

        protected StrainSkill(Mod[] mods, int sectionLength = 400)
            : base(mods)
        {
            StrainPeaks = new SectionPeaks(StrainAtTime, sectionLength);
        }

        protected abstract double StrainValueAt(DifficultyHitObject hitObject);
        protected abstract double StrainAtTime(double time);

        protected override void Process(DifficultyHitObject hitObject)
        {
            StrainPeaks.AdvanceTime(hitObject.StartTime);
            StrainPeaks.UpdateValue(StrainValueAt(hitObject));
        }

        public override double DifficultyValue()
        {
            return StrainPeaks.SortedExponentialWeightedSum(DifficultySumWeight);
        }

        public IEnumerable<double> GetCurrentStrainPeaks() => StrainPeaks;
    }
}
