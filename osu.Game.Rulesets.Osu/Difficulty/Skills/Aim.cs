// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        public Aim(Mod[] mods)
            : base(mods)
        {
        }

        private double currentStrain = 1;

        private double skillMultiplier => 26.25;
        private double strainDecayBase => 0.15;

        private double strainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double aimStrain = 0;

            if (Previous.Count > 0)
            {
                var osuPrevious = (OsuDifficultyHitObject)Previous[0];

                if (osuCurrent.Angle != null && osuCurrent.Angle.Value > angle_bonus_begin)
                {
                    const double scale = 90;

                    double angleBonus = Math.Sqrt(
                        Math.Max(osuPrevious.JumpDistance - scale, 0)
                        * Math.Pow(Math.Sin(osuCurrent.Angle.Value - angle_bonus_begin), 2)
                        * Math.Max(osuCurrent.JumpDistance - scale, 0));
                    aimStrain = 1.4 * applyDiminishingExp(Math.Max(0, angleBonus)) / Math.Max(timing_threshold, osuPrevious.StrainTime);
                }
            }

            double jumpDistanceExp = applyDiminishingExp(osuCurrent.JumpDistance);
            double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance);

            return Math.Max(
                aimStrain + (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(osuCurrent.StrainTime, timing_threshold),
                (Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / osuCurrent.StrainTime
            );
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => currentStrain * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(current) * skillMultiplier;

            return currentStrain;
        }
    }
}
