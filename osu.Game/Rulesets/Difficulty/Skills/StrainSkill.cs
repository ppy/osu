// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class StrainSkill : Skill
    {
        protected virtual double DecayWeight => 0.9;

        protected abstract double SkillMultiplier { get; }

        protected readonly StrainSections sections;

        protected StrainSkill(Mod[] mods, int sectionLength = 400)
            : base(mods)
        {
            sections = new StrainSections(StrainAtTime, sectionLength);
        }

        protected override void Process(DifficultyHitObject hitObject)
        {
            sections.UpdateTime(hitObject.StartTime);
            sections.UpdateStrainPeak(SkillMultiplier*StrainValueAt(hitObject));
        }

        protected abstract double StrainValueAt(DifficultyHitObject hitObject);
        protected abstract double StrainAtTime(double time);


        public override double DifficultyValue()
        {
            return sections.ExponentialWeightedSum(DecayWeight);
        }

        public IEnumerable<double> GetCurrentStrainPeaks() => sections.GetCurrentStrainPeaks();

    }

}
