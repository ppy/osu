// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        public Aim(Mod[] mods)
            : base(mods)
        {
        }

        protected override int HistoryLength => 2;

        private const double wide_angle_multiplier = 1.5;
        private const double acute_angle_multiplier = 2.0;
        private const double slider_multiplier = 1.75;

        private double currentStrain = 1;

        private double skillMultiplier => 23.25;
        private double strainDecayBase => 0.15;

        private double strainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count <= 1 || Previous[0].BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = (OsuDifficultyHitObject)Previous[0];
            var osuLastObj = (OsuDifficultyHitObject)Previous[1];

            double currVelocity = (osuCurrObj.JumpDistance + osuCurrObj.TravelDistance) / osuCurrObj.StrainTime; // Start with the base distance / time

            if (osuPrevObj.BaseObject is Slider) // If object is a slider
            {
                double movementVelocity = osuCurrObj.MovementDistance / osuCurrObj.MovementTime; // calculate the movement velocity from slider end to next note
                double travelVelocity = osuCurrObj.TravelDistance / osuCurrObj.TravelTime; // calculate the slider velocity from slider head to lazy end.

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            double prevVelocity = (osuPrevObj.JumpDistance + osuPrevObj.TravelDistance) / osuPrevObj.StrainTime; // do the same for the previous velocity.

            if (osuLastObj.BaseObject is Slider)
            {
                double movementVelocity = osuPrevObj.MovementDistance / osuPrevObj.MovementTime;
                double travelVelocity = osuPrevObj.TravelDistance / osuPrevObj.TravelTime;

                prevVelocity = Math.Max(prevVelocity, movementVelocity + travelVelocity);
            }

            double angleBonus = 0;
            double sliderBonus = 0;

            double aimStrain = currVelocity; // Start strain with regular velocity.

            if (Precision.AlmostEquals(osuCurrObj.StrainTime, osuPrevObj.StrainTime, 10)) // If rhythms are the same.
            {
                if (osuCurrObj.Angle != null && osuPrevObj.Angle != null)
                {
                    double currAngle = osuCurrObj.Angle.Value;
                    double prevAngle = osuPrevObj.Angle.Value;
                    double lastAngle = osuLastObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    angleBonus = Math.Min(currVelocity, prevVelocity);

                    double wideAngleBonus = calcWideAngleBonus(currAngle);
                    double acuteAngleBonus = calcAcuteAngleBonus(currAngle);

                    if (osuCurrObj.StrainTime > 100) // Only buff deltaTime exceeding 300 bpm 1/2.
                        acuteAngleBonus = 0;
                    else
                        acuteAngleBonus *= calcAcuteAngleBonus(prevAngle) // Multiply by previous angle, we don't want to buff unless this is a wiggle type pattern.
                                            * Math.Min(angleBonus, 125 / osuCurrObj.StrainTime) // The maximum velocity we buff is equal to 125 / strainTime
                                            * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (100 - osuCurrObj.StrainTime) / 25)), 2) // scale buff from 150 bpm 1/4 to 200 bpm 1/4
                                            * Math.Pow(Math.Sin(Math.PI / 2 * (Math.Min(100, osuCurrObj.JumpDistance) - 50) / 50), 2); // Buff distance exceeding 50 (radius) up to 100 (diameter).

                    wideAngleBonus *= angleBonus * (1 - Math.Min(wideAngleBonus, Math.Pow(calcWideAngleBonus(prevAngle), 3))); // Penalize wide angles if they're repeated, reducing the penalty as the prevAngle gets more acute.
                    acuteAngleBonus *= 0.5 + 0.5 * (1 - Math.Min(acuteAngleBonus, Math.Pow(calcAcuteAngleBonus(lastAngle), 3))); // Penalize acute angles if they're repeated, reducing the penalty as the lastAngle gets more obtuse.

                    angleBonus = acuteAngleBonus * acute_angle_multiplier + wideAngleBonus * wide_angle_multiplier; // add the anglebuffs together.
                }
            }

            if (osuCurrObj.TravelTime != 0)
            {
                sliderBonus = osuCurrObj.TravelDistance / osuCurrObj.TravelTime; // add some slider rewards
            }

            aimStrain += angleBonus; // Add in angle bonus.
            aimStrain += sliderBonus * slider_multiplier; // Add in additional slider velocity.

            return aimStrain;
        }

        private double calcWideAngleBonus(double angle) => Math.Pow(Math.Sin(3.0 / 4 * (Math.Min(5.0 / 6 * Math.PI, Math.Max(Math.PI / 6, angle)) - Math.PI / 6)), 2);

        private double calcAcuteAngleBonus(double angle) => 1 - calcWideAngleBonus(angle);

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
