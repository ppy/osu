// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to tap with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class TouchSpeed : Skill
    {
        private const double single_spacing_threshold = 125;

        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        protected override double SkillMultiplier => 1400;
        protected override double StrainDecayBase => 0.3;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45; // ~330BPM
        private const double speed_balancing_factor = 40;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            var osuPrevious = (OsuDifficultyHitObject)current;
            if (Previous.Count > 0)
            {
                osuPrevious = (OsuDifficultyHitObject)Previous[0];
            }
            var streamBonus = 1.0;
            double angle = 0.0;
            double angleBonus = 1.0;
            double distance = osuCurrent.TravelDistance + osuCurrent.JumpDistance;
            double deltaTime = Math.Max(max_speed_bonus, current.DeltaTime);
            double speedBonus = 1.0;
            if (deltaTime < min_speed_bonus)
                speedBonus = 1 + Math.Pow((min_speed_bonus - deltaTime) / speed_balancing_factor, 2);

            double speedValue;
            if (distance > 125)
                speedValue = 1.85;
            else if (distance > 110)
                speedValue = 1.5 + 0.35 * (distance - 110) / 15;
            else if (distance > 90)
                speedValue = 1.2 + 0.3 * (distance - 90) / 20;
            else if (distance > 45)
                speedValue = 0.95 + 0.25 * (distance - 45) / 45;
            else
                speedValue = 0.95;
            // Angle bonus for fast jumps - wide angles generally require you to tapping multiple times with one hand
            if (osuCurrent.Angle != null)
            {
                angle = osuCurrent.Angle.Value;
                angle *= (Math.Max(0, Math.Min(1, (-1 * osuPrevious.StrainTime / 75 + 4)))); // Bonus is reduced if previous two objects are too slow
                angleBonus *= ((0.5 / (1 + Math.Pow(3, -2 * (angle - 2.4)))) + 1);
                angleBonus = Math.Max(1, angleBonus);
            }
            // Bonus for streams specifically
            if (Previous.Count > 1)
            {
                streamBonus = Math.Max(0.0, ((1.2 * (Math.Max(0, Math.Min(0.016 * distance, 2) - 0.7)))) // Adding a bonus to streams if they are high spacing
                * Math.Max(0.0, Math.Min(1, (Math.Max(2.1, 0.01 * angle * (180 / 3.14) + 1.5) - 2))) // Adding a bonus to streams if angle is wide enough
                * Math.Min(1, (2.5 / (1 + Math.Pow(1.05, Math.Min(131, osuCurrent.StrainTime) - 70))))); // Adding a bonus to streams if they are unsingletappable
                streamBonus *= Math.Max(0, Math.Min(1, (-1 * osuPrevious.StrainTime / 75 + 4))); // Bonus is reduced if previous two objects are too slow
                streamBonus = Math.Min(1, Math.Max(0, streamBonus)) + 1;
            }
            return (speedBonus * streamBonus * angleBonus * speedValue) / osuCurrent.StrainTime;
        }
    }
}
