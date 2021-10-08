// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Convenience base class for a <see cref="Skill"/> making use of <see cref="DecayingStrainPeaks"/>
    /// </summary>
    public abstract class StrainDecaySkill : Skill
    {
        protected virtual double DifficultySumWeight => 0.9;

        protected abstract double SkillMultiplier { get; }

        protected readonly DecayingStrainPeaks Strain;

        protected StrainDecaySkill(Mod[] mods, double strainDecayBase, int sectionLength = 400)
            : base(mods)
        {
            Strain = new DecayingStrainPeaks(strainDecayBase, sectionLength);
        }

        protected sealed override void Process(DifficultyHitObject hitObject)
        {
            Strain.IncrementStrainAtTime(hitObject.StartTime, SkillMultiplier * StrainValueOf(hitObject));
        }

        protected abstract double StrainValueOf(DifficultyHitObject hitObject);

        public override double DifficultyValue()
        {
            return Strain.StrainPeaks.SortedExponentialWeightedSum(DifficultySumWeight);
        }

        public IEnumerable<double> GetCurrentStrainPeaks() => Strain.StrainPeaks;
    }
}
