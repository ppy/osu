using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class AimAngleBonus : PreStrainSkill
    {
        private const double wide_angle_multiplier = 1.5;
        private const double acute_angle_multiplier = 2.0;

        private AimVelocity aimVelocity;
        
        protected override double SkillMultiplier => 1.0;

        protected override double StrainDecayBase => 0.0;

        protected override int HistoryLength => 2;

        public AimAngleBonus(AimVelocity aimVelocity, Mod[] mods) : base(mods)
        {
            this.aimVelocity = aimVelocity;
        }

        protected override double StrainValueOf(int index, DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count <= 1 || Previous[0].BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)Previous[0];
            var osuLastLastObj = (OsuDifficultyHitObject)Previous[1];

            double currVelocity = aimVelocity[index];
            double prevVelocity = aimVelocity[index - 1];

            double angleBonus = 0;

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

                    angleBonus = acuteAngleBonus * acute_angle_multiplier + wideAngleBonus * wide_angle_multiplier; // add the angle buffs together.
                }
            }

            return angleBonus;
        }

        private double calcWideAngleBonus(double angle) => Math.Pow(Math.Sin(3.0 / 4 * (Math.Min(5.0 / 6 * Math.PI, Math.Max(Math.PI / 6, angle)) - Math.PI / 6)), 2);

        private double calcAcuteAngleBonus(double angle) => 1 - calcWideAngleBonus(angle);
    }
}
