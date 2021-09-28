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
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private const double single_spacing_threshold = 125;

        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        protected override double SkillMultiplier => 1400;
        protected override double StrainDecayBase => 0.3;
        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        private const double min_doubletap_nerf = 0.9; // minimum speedBonus value (eventually on stacked)
        private const double max_doubletap_nerf = 1.0; // maximum speedBonus value 
        private const double threshold_fully_contributing = 0.75; // minimum distance not influenced

        private readonly double greatWindow;


        public Speed(Mod[] mods, double hitWindowGreat)
            : base(mods)
        {
            greatWindow = hitWindowGreat;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            var osuPrevious = Previous.Count > 0 ? (OsuDifficultyHitObject)Previous[0] : null;

            double distance = Math.Min(single_spacing_threshold, osuCurrent.TravelDistance + osuCurrent.JumpDistance);
            double strainTime = osuCurrent.StrainTime;

            double greatWindowFull = greatWindow * 2;
            double speedWindowRatio = strainTime / greatWindowFull;

            // Aim to nerf cheesy rhythms (Very fast consecutive doubles with large deltatimes between)
            if (osuPrevious != null && strainTime < greatWindowFull && osuPrevious.StrainTime > strainTime)
                strainTime = Interpolation.Lerp(osuPrevious.StrainTime, strainTime, speedWindowRatio);

            // Cap deltatime to the OD 300 hitwindow.
            // 0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
            strainTime /= Math.Clamp((strainTime / greatWindowFull) / 0.93, 0.92, 1);



            double speedBonus = 1.0;
            if (strainTime < min_speed_bonus)
            {
                double radius = ((OsuHitObject)osuCurrent.BaseObject).Radius;
                /* a basecode to nerf acute high bpm stream and doubletap. 
                 * because no one could do doubletap on spaced streams.
                 * finally it is multiplied on speedBonus. 
                 *
                 * but this code make some speed players discourage.
                 * it makes hidamari no uta be taken about 150pp.
                 * and other stream maps be taken about 0-5pp in average. 
                 
                 ** We could consider to apply this code on angleBonus **/
                double multiplierForSpeedBonus = min_doubletap_nerf +
                    Math.Min(Math.Max(distance / (radius * threshold_fully_contributing), 1.0), 0.0)
                    * (max_doubletap_nerf - min_doubletap_nerf)
                    ;
                speedBonus = 1 + Math.Pow((min_speed_bonus - strainTime) / speed_balancing_factor, 2)
                                    * multiplierForSpeedBonus;
            }


            double angleBonus = 1.0;

            if (osuCurrent.Angle != null && osuCurrent.Angle.Value < angle_bonus_begin)
            {
                angleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - osuCurrent.Angle.Value)), 2) / 3.57;

                if (osuCurrent.Angle.Value < pi_over_2)
                {
                    angleBonus = 1.28;
                    if (distance < 90 && osuCurrent.Angle.Value < pi_over_4)
                        angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1);
                    else if (distance < 90)
                        angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1) * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4);
                }
            }

            return (1 + (speedBonus - 1) * 0.75)
                   * angleBonus
                   * (0.95 + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5))
                   / strainTime
                   //* multiplier
                   ;
        }
    }
}
