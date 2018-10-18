// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        private const double single_spacing_threshold = 125;
        private const double stream_spacing_threshold = 110;
        private const double almost_diameter = 90;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double distance = current.TravelDistance + current.JumpDistance;

            double speedBonus = 1.0;
            if (current.StrainTime < 68) // 68 = 220 BPM 1/4th snapping in MS.
                speedBonus = 68 / current.StrainTime; // 1.09x for 240 BPM, 1.18x for 260 BPM, 1.36x for 300 BPM, etc.

            double speedValue;
            if (distance > single_spacing_threshold)
                speedValue = 2.5;
            else if (distance > stream_spacing_threshold)
                speedValue = 1.6 + 0.9 * (distance - stream_spacing_threshold) / (single_spacing_threshold - stream_spacing_threshold);
            else if (distance > almost_diameter)
                speedValue = 1.2 + 0.4 * (distance - almost_diameter) / (stream_spacing_threshold - almost_diameter);
            else if (distance > almost_diameter / 2)
                speedValue = 0.95 + 0.25 * (distance - almost_diameter / 2) / (almost_diameter / 2);
            else
                speedValue = 0.95;

            return speedValue * speedBonus / current.StrainTime;
        }
    }
}
