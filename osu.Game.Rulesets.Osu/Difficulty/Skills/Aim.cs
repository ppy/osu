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
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        public Aim(Mod[] mods)
            : base(mods)
        {
        }

        protected override int HistoryLength => 2;

        protected override double SkillMultiplier => 24.75;
        protected override double StrainDecayBase => 0.15;

        private double wideAngleMultiplier = 1.0;
        private double acuteAngleMultiplier = 1.0;
        private double rhythmVarianceMultiplier = 1.0;
        private double sliderMultiplier = 6.5;
        private double sliderJumpMultiplier = 0.875;
        private double velChangeMultiplier = 7.5;

        private double calcWideAngleBonus(double angle)
        {
            if (angle < Math.PI / 3)
                return 0;
            if (angle < 2 * Math.PI / 3)
                return Math.Pow(Math.Sin(1.5 * (angle - Math.PI / 3)), 2);
            else
                return 0.25 + 0.75 * Math.Pow(Math.Sin(1.5 * (Math.PI / 3 - (angle - 2 * Math.PI / 3))), 2);
        }

        private double calcAcuteAngleBonus(double angle)
        {
            if (angle < Math.PI / 3)
                return 0.5 + 0.5 * Math.Pow(Math.Sin(1.5 * angle), 2);
            if (angle < 2 * Math.PI / 3)
                return Math.Pow(Math.Sin(1.5 * (Math.PI / 3 - (angle - Math.PI / 3))), 2);
            else
                return 0;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            double aimStrain = 0;

            if (Previous.Count > 1)
            {
                var osuCurrObj = (OsuDifficultyHitObject)current;
                var osuPrevObj = (OsuDifficultyHitObject)Previous[0];
                var osuLastObj = (OsuDifficultyHitObject)Previous[1];

                var currVector = Vector2.Divide(osuCurrObj.JumpVector, (float)osuCurrObj.StrainTime);
                var prevVector = Vector2.Divide(osuPrevObj.JumpVector, (float)osuPrevObj.StrainTime);
                // var lastVector = Vector2.Divide(osuLastObj.JumpVector, (float)osuLastObj.StrainTime); // not needed right now.

                if (Precision.AlmostEquals(osuCurrObj.StrainTime, osuPrevObj.StrainTime, 10)) // Rhythms are the same.
                {
                    if (osuCurrObj.Angle != null)
                    {
                        double angle = osuCurrObj.Angle.Value;
                        double angleBonus = Math.Min(currVector.Length, prevVector.Length); // Rewarding angles, take the smaller velocity as base.

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
                            angleBonus = Math.Min(angleBonus, 150 / osuCurrObj.StrainTime) * Math.Min(1, Math.Pow(Math.Min(osuCurrObj.JumpDistance, osuPrevObj.JumpDistance) / 150, 2));;

                        angleBonus *= Math.Max(acuteAngleBonus * acuteAngleMultiplier, wideAngleBonus * wideAngleMultiplier);

                        aimStrain += angleBonus; // add in angle velocity.
                    }

                    if (prevVector.Length > currVector.Length)
                    {
                        double velChangeBonus = Math.Max(0, Math.Sqrt((prevVector.Length - currVector.Length) * currVector.Length) - currVector.Length) * Math.Min(1, osuCurrObj.JumpDistance / 100);

                        aimStrain += velChangeBonus * velChangeMultiplier * Math.Sqrt(100 / osuCurrObj.StrainTime);
                    }
                }
                else // There is a rhythm change
                {
                    double rhythmBonus = Math.Min(currVector.Length, prevVector.Length); // Rewarding rhythm, take the smaller velocity as base.

                    if (osuCurrObj.StrainTime + 10 < osuPrevObj.StrainTime && osuPrevObj.StrainTime > osuLastObj.StrainTime + 10)
                        rhythmBonus = 0; // Don't want to reward for a rhythm change back to back (unless its a double, which is why this only checks for fast -> slow -> fast).

                    aimStrain += rhythmBonus * rhythmVarianceMultiplier; // add in rhythm velocity.
                }

                if (osuCurrObj.TravelDistance != 0)
                {
                    double sliderBonus = Math.Max(osuCurrObj.TravelDistance, sliderJumpMultiplier * Math.Sqrt(osuCurrObj.TravelDistance * osuCurrObj.JumpDistance)) / osuCurrObj.StrainTime;

                    aimStrain += sliderBonus * sliderMultiplier; // Add in slider velocity.
                }

                aimStrain += currVector.Length; // Add in regular velocity.
            }

            return aimStrain;
        }
    }
}
