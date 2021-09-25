// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Framework.Utils;
using osuTK;

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

        protected override double SkillMultiplier => 24.75;
        protected override double StrainDecayBase => 0.15;

        private const double wide_angle_multiplier = 1.0;
        private const double acute_angle_multiplier = 1.0;
        private const double rhythm_variance_multiplier = 1.0;
        private const double vel_change_multiplier = 2.0;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count <= 1)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = (OsuDifficultyHitObject)Previous[0];
            var osuLastObj = (OsuDifficultyHitObject)Previous[1];

            var currVector = Vector2.Divide(osuCurrObj.JumpVector, (float)osuCurrObj.StrainTime);
            var prevVector = Vector2.Divide(osuPrevObj.JumpVector, (float)osuPrevObj.StrainTime);

            // Start with regular velocity.
            double aimStrain = currVector.Length;

            if (Precision.AlmostEquals(osuCurrObj.StrainTime, osuPrevObj.StrainTime, 10)) // Rhythms are the same.
            {
                if (osuCurrObj.Angle != null)
                {
                    double angle = osuCurrObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    double angleBonus = Math.Min(currVector.Length, prevVector.Length);

                    double wideAngleBonus = calcWideAngleBonus(angle);
                    double acuteAngleBonus = calcAcuteAngleBonus(angle);

                    if (osuCurrObj.StrainTime > 100)
                        acuteAngleBonus = 0;
                    else
                    {
                        acuteAngleBonus *= Math.Min(2, Math.Pow((100 - osuCurrObj.StrainTime) / 15, 1.5));
                        wideAngleBonus *= Math.Pow(osuCurrObj.StrainTime / 100, 6);
                    }

                    if (acuteAngleBonus > wideAngleBonus)
                        angleBonus = Math.Min(angleBonus, 150 / osuCurrObj.StrainTime) * Math.Min(1, Math.Pow(Math.Min(osuCurrObj.JumpDistance, osuPrevObj.JumpDistance) / 150, 2));

                    angleBonus *= Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier);

                    // add in angle velocity.
                    aimStrain += angleBonus;
                }

                if (prevVector.Length > currVector.Length)
                {
                    double velChangeBonus = Math.Max(0, Math.Sqrt((prevVector.Length - currVector.Length) * currVector.Length) - Math.Max(0, currVector.Length - 100 / osuCurrObj.StrainTime)) * Math.Min(1, osuCurrObj.JumpDistance / 100);

                    aimStrain += velChangeBonus * vel_change_multiplier;
                }
            }
            else // There is a rhythm change
            {
                // Rewarding rhythm, take the smaller velocity as base.
                double rhythmBonus = Math.Min(currVector.Length, prevVector.Length);

                if (osuCurrObj.StrainTime + 10 < osuPrevObj.StrainTime && osuPrevObj.StrainTime > osuLastObj.StrainTime + 10)
                    // Don't want to reward for a rhythm change back to back (unless its a double, which is why this only checks for fast -> slow -> fast).
                    rhythmBonus = 0;

                aimStrain += rhythmBonus * rhythm_variance_multiplier; // add in rhythm velocity.
            }

            return aimStrain;
        }

        private double calcWideAngleBonus(double angle)
        {
            if (angle < Math.PI / 3)
                return 0;
            if (angle < 2 * Math.PI / 3)
                return Math.Pow(Math.Sin(1.5 * (angle - Math.PI / 3)), 2);

            return 0.25 + 0.75 * Math.Pow(Math.Sin(1.5 * (Math.PI - angle)), 2);
        }

        private double calcAcuteAngleBonus(double angle)
        {
            if (angle < Math.PI / 3)
                return 0.5 + 0.5 * Math.Pow(Math.Sin(1.5 * angle), 2);
            if (angle < 2 * Math.PI / 3)
                return Math.Pow(Math.Sin(1.5 * (2 * Math.PI / 3 - angle)), 2);

            return 0;
        }
    }
}
