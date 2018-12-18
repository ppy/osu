// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : Skill
    {
        protected override double SkillMultiplier => 1400;
        protected override double StrainDecayBase => 0.3;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double distance = Math.Min(SINGLE_SPACING_THRESHOLD, current.TravelDistance + current.JumpDistance);
            return (0.95 + Math.Pow(distance / SINGLE_SPACING_THRESHOLD, 4)) / current.StrainTime;
        }
    }
}
