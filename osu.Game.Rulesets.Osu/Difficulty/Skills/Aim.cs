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
        private const double skill_multipler = 26.25;

        public Aim(Mod[] mods)
            : base(mods: mods, strainDecayBase: 0.15)
        {
        }

        protected override void Process(DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;
            osuCurrent.Aim = new HitObjectAttributes(current, this);
        }

        public struct HitObjectAttributes
        {
            public double AngleBonus;
            public double TotalDistance;
            public double TravelDistanceExp;

            public double Strain;
            public double CumulativeStrain;

            public HitObjectAttributes(DifficultyHitObject current, Aim state)
                : this()
            {
                if (current.BaseObject is Spinner)
                    return;

                var osuCurrent = (OsuDifficultyHitObject)current;

                if (state.Previous.Count > 0)
                {
                    var osuPrevious = (OsuDifficultyHitObject)state.Previous[0];

                    if (osuCurrent.Angle != null && osuCurrent.Angle.Value > angle_bonus_begin)
                    {
                        const double scale = 90;

                        var rawAngleBonus = Math.Sqrt(
                            Math.Max(osuPrevious.JumpDistance - scale, 0)
                            * Math.Pow(Math.Sin(osuCurrent.Angle.Value - angle_bonus_begin), 2)
                            * Math.Max(osuCurrent.JumpDistance - scale, 0));
                        AngleBonus = 1.4 * applyDiminishingExp(Math.Max(0, rawAngleBonus)) / Math.Max(timing_threshold, osuPrevious.StrainTime);
                    }
                }

                double jumpDistanceExp = applyDiminishingExp(osuCurrent.JumpDistance);
                double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance);
                TotalDistance = jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp);

                Strain = skill_multipler * Math.Max(
                    AngleBonus + TotalDistance / Math.Max(osuCurrent.StrainTime, timing_threshold),
                    TotalDistance / osuCurrent.StrainTime
                );
                CumulativeStrain = state.IncrementStrainAtTime(osuCurrent.StartTime, Strain);
            }
        }

        private static double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
