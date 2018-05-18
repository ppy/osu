// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : Skill
    {
        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        private const double single_spacing_threshold = 125;
        private const double stream_spacing_threshold = 110;
        private const double almost_diameter = 90;

        protected override double StrainValueOf(OsuDifficultyHitObject current)
        {
            double distance = current.Distance;

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
                flowValue = 0.0;

            return flowValue / current.DeltaTime;
        }
    }
    {
        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(OsuDifficultyHitObject current) => Math.Pow(current.Distance, 0.99) / current.DeltaTime;
    }
}
