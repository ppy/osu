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
        public Aim(Mod[] mods)
            : base(mods)
        {
        }

        protected override int HistoryLength => 2;

        private const double wide_angle_multiplier = 1.5;
        private const double acute_angle_multiplier = 2.0;
        private const double slider_multiplier = 1.5;
        private const double vel_change_multiplier = 1.0;

        private double currentStrain = 1;

        private double skillMultiplier => 23.25;
        private double strainDecayBase => 0.15;

        private double strainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count <= 1 || Previous[0].BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)Previous[0];
            var osuLastLastObj = (OsuDifficultyHitObject)Previous[1];

            // Calculate the velocity to the current hitobject, which starts with a base distance / time assuming the last object is a hitcircle.
            double currVelocity = osuCurrObj.JumpDistance / osuCurrObj.StrainTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider)
            {
                double movementVelocity = osuCurrObj.MovementDistance / osuCurrObj.MovementTime; // calculate the movement velocity from slider end to current object
                double travelVelocity = osuCurrObj.TravelDistance / osuCurrObj.TravelTime; // calculate the slider velocity from slider head to slider end.

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            // As above, do the same for the previous hitobject.
            double prevVelocity = osuLastObj.JumpDistance / osuLastObj.StrainTime;

            if (osuLastLastObj.BaseObject is Slider)
            {
                double movementVelocity = osuLastObj.MovementDistance / osuLastObj.MovementTime;
                double travelVelocity = osuLastObj.TravelDistance / osuLastObj.TravelTime;

                prevVelocity = Math.Max(prevVelocity, movementVelocity + travelVelocity);
            }

            double angleBonus = 0;
            double sliderBonus = 0;
            double velChangeBonus = 0;

            double aimStrain = currVelocity; // Start strain with regular velocity.

            if (Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime) < 1.25 * Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime)) // If rhythms are the same.
            {
                if (osuCurrObj.Angle != null && osuLastObj.Angle != null && osuLastLastObj.Angle != null)
                {
                    double currAngle = osuCurrObj.Angle.Value;
                    double lastAngle = osuLastObj.Angle.Value;
                    double lastLastAngle = osuLastLastObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    angleBonus = Math.Min(currVelocity, prevVelocity);

                    double wideAngleBonus = calcWideAngleBonus(currAngle);
                    double acuteAngleBonus = calcAcuteAngleBonus(currAngle);

                    if (osuCurrObj.StrainTime > 100) // Only buff deltaTime exceeding 300 bpm 1/2.
                        acuteAngleBonus = 0;
                    else
                    {
                        acuteAngleBonus *= calcAcuteAngleBonus(lastAngle) // Multiply by previous angle, we don't want to buff unless this is a wiggle type pattern.
                                           * Math.Min(angleBonus, 125 / osuCurrObj.StrainTime) // The maximum velocity we buff is equal to 125 / strainTime
                                           * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (100 - osuCurrObj.StrainTime) / 25)), 2) // scale buff from 150 bpm 1/4 to 200 bpm 1/4
                                           * Math.Pow(Math.Sin(Math.PI / 2 * (Math.Clamp(osuCurrObj.JumpDistance, 50, 100) - 50) / 50), 2); // Buff distance exceeding 50 (radius) up to 100 (diameter).
                    }

                    wideAngleBonus *= angleBonus * (1 - Math.Min(wideAngleBonus, Math.Pow(calcWideAngleBonus(lastAngle), 3))); // Penalize wide angles if they're repeated, reducing the penalty as the lastAngle gets more acute.
                    acuteAngleBonus *= 0.5 + 0.5 * (1 - Math.Min(acuteAngleBonus, Math.Pow(calcAcuteAngleBonus(lastLastAngle), 3))); // Penalize acute angles if they're repeated, reducing the penalty as the lastLastAngle gets more obtuse.

                    angleBonus = Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier); // Take the max of the multipliers.
                }
            }

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                prevVelocity = (osuLastObj.JumpDistance + osuLastObj.TravelDistance) / osuLastObj.StrainTime; // We want to use the average velocity when awarding differences, not necessarily combined.
                currVelocity = (osuCurrObj.JumpDistance + osuCurrObj.TravelDistance) / osuCurrObj.StrainTime;

                velChangeBonus = Math.Max(Math.Abs(prevVelocity - currVelocity) // reward for % distance slowed down compared to previous, paying attention to not award overlap
                                          * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, osuCurrObj.JumpDistance / 100)), 2) // do not award overlap
                                          * Math.Pow(Math.Sin(Math.PI / 2 * Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity)), 2), // scale with ratio of difference compared to max
                                          Math.Min(125 / Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime), Math.Abs(prevVelocity - currVelocity)) // reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                                          * Math.Pow(Math.Sin(Math.PI / 2 * Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity)), 2)); // scale with ratio of difference compared to max

                velChangeBonus *= Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2); // penalize for rhythm changes.
            }

            if (osuCurrObj.TravelTime != 0)
            {
                sliderBonus = osuCurrObj.TravelDistance / osuCurrObj.TravelTime; // add some slider rewards
            }

            aimStrain += Math.Max(angleBonus, velChangeBonus * vel_change_multiplier); // Add in angle bonus or velchange bonus, whichever is larger.
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
