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
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class TouchAim : Skill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            var jump = osuCurrent.JumpDistance;

            if (Previous.Count > 1)
            {
                var osuPrevious = (OsuDifficultyHitObject)Previous[0];
                var angle = osuCurrent.Angle.Value;
                angle *= (Math.Max(0, Math.Min(1, (-1 * osuPrevious.StrainTime / 50 + 4))));
                angle *= (Math.Min(Math.Max(osuPrevious.JumpDistance / 50 - 2.25, 0), 1));
                jump *= ((2 / (1 + Math.Pow(3, -1 * angle))) - 0.075);
                jump *= (1 + (1.75 / (1 + Math.Pow(1.06, (2.5 * Math.Min(osuPrevious.StrainTime, 90) - 120))))
                    * (Math.Min(1, Math.Max(0, -0.015 * osuPrevious.JumpDistance + 1.6))));
            }

            double jumpDistanceExp = applyDiminishingExp(jump);
            double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance);

            return Math.Max(
                (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(osuCurrent.StrainTime, timing_threshold),
                (Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / osuCurrent.StrainTime
            );
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
