// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Stamina : Speed
    {
        protected override double SkillMultiplier => base.SkillMultiplier * 0.78;
        protected override double StrainDecayBase => base.StrainDecayBase * 1.4;
        public Stamina(Mod[] mods) : base(mods)
        {

        }

        protected override double CalculateEstimatedPeakStrain(double strainTime)
        {
            return Math.Pow(1200 / strainTime, 1.8) + Math.Pow(225 / strainTime, 3.4);
        }
    }
}
