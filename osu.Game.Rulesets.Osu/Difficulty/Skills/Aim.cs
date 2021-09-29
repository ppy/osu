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

        private const double wide_angle_multiplier = 2.25;
        private const double acute_angle_multiplier = 1.5;
        private const double rhythm_variance_multiplier = 1.0;
        private const double vel_change_multiplier = 1.0;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count <= 1)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = (OsuDifficultyHitObject)Previous[0];
            var osuLastObj = (OsuDifficultyHitObject)Previous[1];

            var currVector = Vector2.Divide(osuCurrObj.JumpVector, (float)osuCurrObj.StrainTime);
            var prevVector = Vector2.Divide(osuPrevObj.JumpVector, (float)osuPrevObj.StrainTime);

            double angleBonus = 0;
            double rhythmBonus = 0;
            double velChangeBonus = 0;
            // double angleBonus = 0;

            // Start with regular velocity.
            double aimStrain = currVector.Length;

            if (Precision.AlmostEquals(osuCurrObj.StrainTime, osuPrevObj.StrainTime, 10)) // Rhythms are the same.
            {
                if (osuCurrObj.Angle != null)
                {
                    double angle = osuCurrObj.Angle.Value;

                    // Rewarding angles, take the smaller velocity as base.
                    angleBonus = Math.Min(currVector.Length, prevVector.Length);

                    double wideAngleBonus = calcWideAngleBonus(angle);
                    double acuteAngleBonus = calcAcuteAngleBonus(angle);

                    if (osuCurrObj.StrainTime > 100) // 150 BPM 1/4
                        acuteAngleBonus = 0;
                    else
                        acuteAngleBonus *= Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (100 - osuCurrObj.StrainTime) / 25)), 2); // sin curve from 150 bpm 1/4 to 200 bpm 1/4


                    acuteAngleBonus *= Math.Min(angleBonus, 125 / osuCurrObj.StrainTime) * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (Math.Min(osuCurrObj.JumpDistance, osuPrevObj.JumpDistance) - 50) / 50)), 3); // scale buff to ensure that overlaps are penalize 0:0-50, 50-100 distance = sin curve
                    wideAngleBonus *= wideAngleBonus * Math.Max(0, angleBonus - 125 / osuCurrObj.StrainTime); // remove wide angle buff for the 125 distance area since its near stream / overlap


                    angleBonus = Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier);
                }
            }
            else // There is a rhythm change
            {
                // Rewarding rhythm, take the smaller velocity as base.
                rhythmBonus = Math.Min(currVector.Length, prevVector.Length);

                if (osuCurrObj.StrainTime + 10 < osuPrevObj.StrainTime && osuPrevObj.StrainTime > osuLastObj.StrainTime + 10)
                    // Don't want to reward for a rhythm change back to back (unless its a double, which is why this only checks for fast -> slow -> fast).
                    rhythmBonus = 0;
            }

            if (prevVector.Length > currVector.Length && prevVector.Length > 0)
            {
                velChangeBonus = (prevVector.Length - currVector.Length) * Math.Sqrt(100 / osuCurrObj.StrainTime)
                                                  * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, osuCurrObj.JumpDistance / 125)), 2)
                                                  * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (prevVector.Length - currVector.Length) / prevVector.Length)), 2);

                if (Precision.AlmostEquals(osuCurrObj.StrainTime, osuPrevObj.StrainTime, 10))
                    velChangeBonus = Math.Max(velChangeBonus,
                                                  (Math.Min(125 / osuCurrObj.StrainTime, prevVector.Length) - Math.Min(125 / osuCurrObj.StrainTime, currVector.Length)) * Math.Sqrt(100 / osuCurrObj.StrainTime)
                                                  * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (Math.Min(125 / osuCurrObj.StrainTime, prevVector.Length) - Math.Min(125 / osuCurrObj.StrainTime, currVector.Length)) / prevVector.Length)), 2));
            }
            else if (currVector.Length > 0)
            {
                velChangeBonus = (currVector.Length - prevVector.Length) * Math.Sqrt(100 / osuCurrObj.StrainTime)
                                                  * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, osuPrevObj.JumpDistance / 125)), 2)
                                                  * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (currVector.Length - prevVector.Length) / currVector.Length)), 2);

                if (Precision.AlmostEquals(osuCurrObj.StrainTime, osuPrevObj.StrainTime, 10))
                    velChangeBonus = Math.Max(velChangeBonus,
                                                  (Math.Min(125 / osuCurrObj.StrainTime, currVector.Length) - Math.Min(125 / osuCurrObj.StrainTime, prevVector.Length)) * Math.Sqrt(100 / osuCurrObj.StrainTime)
                                                  * Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(1, (Math.Min(125 / osuCurrObj.StrainTime, currVector.Length) - Math.Min(125 / osuCurrObj.StrainTime, prevVector.Length)) / prevVector.Length)), 2));
            }

            // add in angle velocity.
            aimStrain += angleBonus;
            aimStrain += Math.Max(rhythmBonus * rhythm_variance_multiplier, velChangeBonus * vel_change_multiplier); // add in rhythm velocity.
            // aimStrain += velChangeBonus * vel_change_multiplier;

            return aimStrain;
        }

        private double calcWideAngleBonus(double angle)
        {
            if (angle < Math.PI / 4)
                return 0;
            if (angle < 3 * Math.PI / 4)
                return Math.Pow(Math.Sin((angle - Math.PI / 4)), 2);

            return 0.5 + 0.5 * Math.Pow(Math.Sin(2 * (Math.PI - angle)), 2);
        }

        private double calcAcuteAngleBonus(double angle)
        {
            if (angle < Math.PI / 4)
                return 0.75 + 0.25 * Math.Pow(Math.Sin(2 * angle), 2);
            if (angle < 3 * Math.PI / 4)
                return 1.0 - Math.Pow(Math.Sin(angle - Math.PI / 4), 2);

            return 0;
        }
    }
}
