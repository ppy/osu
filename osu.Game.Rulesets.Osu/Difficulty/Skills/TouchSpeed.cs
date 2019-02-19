// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45; // ~330BPM
        private const double speed_balancing_factor = 40;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double distance = Math.Min(SINGLE_SPACING_THRESHOLD, current.TravelDistance + current.JumpDistance);
            double deltaTime = Math.Max(max_speed_bonus, current.DeltaTime);
            distance = max(distance, distance * ((0.5 / 1 + Math.Pow(2.71, -0.5 * current.Angle.Value))) + 0.75))
           

            double speedBonus = 1.0;
            if (deltaTime < min_speed_bonus)
                speedBonus = 1 + Math.Pow((min_speed_bonus - deltaTime) / speed_balancing_factor, 2);
                
            double touchBonus = ((Math.Max(0, Math.Min(0.012 * distance, 1.8) - 0.7)) // Distance Bonus
            * Math.Min(1, (Math.Max(2.1, 0.01 * current.Angle.Value * (180 / 3.14) + 1.5) - 2)) // Angle Bonus
            * (2.4 / (1 + Math.Pow(1.065, Math.Min(131, deltaTime) - 70)))); // Speed Bonus
            
            touchBonus = touchBonus * Math.Max(0,Math.Min(1,(-1 * Previous[0].StrainTime / 50 + 4))); 
            
            touchBonus = 1.25 * Math.Max(0, touchBonus) + 1
            //Indirectly lower stream bonus depending on time between last two objects

            double speedValue;
            if (distance > 125)
                speedValue = 2.5;
            else if (distance > 110)
                speedValue = 1.6 + 0.9 * (distance - 110) / (15);
            else if (distance > 90)
                speedValue = 1.2 + 0.4 * (distance - 90) / (20);
            else if (distance > 45)
                speedValue = 0.95 + 0.25 * (distance - 45) / (45);
            else
                speedValue = 0.95;

            return touchBonus * speedBonus * speedValue / current.StrainTime;

        }
    }
}
