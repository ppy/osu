// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Tau.Difficulty.Skills
{
    public class SkillPlaceHolder : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            return 1.0;
        }
    }
}
