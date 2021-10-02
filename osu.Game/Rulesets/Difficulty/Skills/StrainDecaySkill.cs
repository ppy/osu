// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class StrainDecaySkill : Skill
    {
        protected virtual double DecayWeight => 0.9;

        protected abstract double SkillMultiplier { get; }

        protected readonly DecayingStrainSections strain;

        protected StrainDecaySkill(Mod[] mods, double strainDecayBase, int sectionLength=400)
            : base(mods)
        {
            strain = new DecayingStrainSections(strainDecayBase, sectionLength);
        }

        protected override void Process(DifficultyHitObject hitObject)
        {
            strain.AddStrain(hitObject.StartTime, SkillMultiplier*StrainValueOf(hitObject));
        }

        protected abstract double StrainValueOf(DifficultyHitObject hitObject);

        public override double DifficultyValue()
        {
            return strain.sections.ExponentialWeightedSum(DecayWeight);
        }

        public IEnumerable<double> GetCurrentStrainPeaks() => strain.sections.GetCurrentStrainPeaks();

    }
}
