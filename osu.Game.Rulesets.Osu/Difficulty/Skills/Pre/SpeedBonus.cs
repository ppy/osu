// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class SpeedBonus : PreStrainSkill
    {
        private const double single_spacing_threshold = 125;
        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        protected override double StrainDecayBase => 0.0;

        protected override double SkillMultiplier => 1.0;

        private readonly SpeedStrainTime speedStrainTime;

        public SpeedBonus(Mod[] mods, SpeedStrainTime speedStrainTime)
            : base(mods)
        {
            this.speedStrainTime = speedStrainTime;
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = Previous.Count > 0 ? (OsuDifficultyHitObject)Previous[0] : null;

            double strainTime = speedStrainTime.GetCurrentStrain();

            double speedBonus = 1.0;

            if (strainTime < min_speed_bonus)
                speedBonus = 1 + 0.75 * Math.Pow((min_speed_bonus - strainTime) / speed_balancing_factor, 2);

            double travelDistance = osuPrevObj?.TravelDistance ?? 0;
            double distance = Math.Min(single_spacing_threshold, travelDistance + osuCurrObj.MinimumJumpDistance);

            return (speedBonus + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / strainTime;
        }
    }
}
