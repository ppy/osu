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
        private const double skill_multiplier = 1400;

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        private readonly double greatWindow;

        public Speed(Mod[] mods, double hitWindowGreat)
            : base(mods, strainDecayBase: 0.3)
        {
            greatWindow = hitWindowGreat;
        }

        protected override void Process(DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;

            osuCurrent.Speed = new HitObjectAttributes(current, this);
        }

        public struct HitObjectAttributes
        {
            public double CappedStrainTime;
            public double AngleBonus;
            public double SpeedBonus;

            public double Strain;
            public double CumulativeStrain;

            public HitObjectAttributes(DifficultyHitObject current, Speed state)
                : this()
            {
                if (current.BaseObject is Spinner)
                    return;

                var osuCurrent = (OsuDifficultyHitObject)current;
                var osuPrevious = state.Previous.Count > 0 ? (OsuDifficultyHitObject)state.Previous[0] : null;

                double distance = Math.Min(single_spacing_threshold, osuCurrent.TravelDistance + osuCurrent.JumpDistance);

                double greatWindowFull = state.greatWindow * 2;
                double speedWindowRatio = osuCurrent.StrainTime / greatWindowFull;
                CappedStrainTime = osuCurrent.StrainTime;

                // Aim to nerf cheesy rhythms (Very fast consecutive doubles with large deltatimes between)
                if (osuPrevious != null && CappedStrainTime < greatWindowFull && osuPrevious.StrainTime > CappedStrainTime)
                    CappedStrainTime = Interpolation.Lerp(osuPrevious.StrainTime, CappedStrainTime, speedWindowRatio);

                // Cap deltatime to the OD 300 hitwindow.
                // 0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
                CappedStrainTime /= Math.Clamp((CappedStrainTime / greatWindowFull) / 0.93, 0.92, 1);

                SpeedBonus = 1.0;
                if (CappedStrainTime < min_speed_bonus)
                    SpeedBonus = 1 + Math.Pow((min_speed_bonus - CappedStrainTime) / speed_balancing_factor, 2);

                AngleBonus = 1.0;

                if (osuCurrent.Angle != null && osuCurrent.Angle.Value < angle_bonus_begin)
                {
                    AngleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - osuCurrent.Angle.Value)), 2) / 3.57;

                    if (osuCurrent.Angle.Value < pi_over_2)
                    {
                        AngleBonus = 1.28;

                        if (distance < 90 && osuCurrent.Angle.Value < pi_over_4)
                            AngleBonus += (1 - AngleBonus) * Math.Min((90 - distance) / 10, 1);
                        else if (distance < 90)
                        {
                            AngleBonus += (1 - AngleBonus)
                                          * Math.Min((90 - distance) / 10, 1)
                                          * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4);
                        }
                    }
                }

                Strain = skill_multiplier * (1 + (SpeedBonus - 1) * 0.75)
                                          * AngleBonus
                                          * (0.95 + SpeedBonus * Math.Pow(distance / single_spacing_threshold, 3.5))
                         / CappedStrainTime;

                CumulativeStrain = state.IncrementStrainAtTime(current.StartTime, Strain);
            }
        }
    }
}
