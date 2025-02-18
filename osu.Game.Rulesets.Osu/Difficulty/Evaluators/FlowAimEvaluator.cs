// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class FlowAimEvaluator
    {
        private static double flowMultiplier => 1.25;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            // Start with velocity
            double flowDifficulty = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;

            // Square the distance to turn into d/t
            flowDifficulty *= osuCurrObj.LazyJumpDistance / diameter;

            double angleBonus = 0;

            if (osuCurrObj.AngleSigned != null && osuLastObj.AngleSigned != null)
            {

                double angleChangeBonus = CalculateFlowAngleChangeBonus(current);
                double acuteAngleBonus = CalculateFlowAcuteAngleBonus(current);

                double streamNerf = 1.0;

                if (osuCurrObj.Index >= 4)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        osuLastObj = (OsuDifficultyHitObject)current.Previous(i);

                        // Angle is constant
                        double angle = osuLastObj.Angle ?? 0;
                        double reverseAngleBonus = 1 - AimEvaluator.CalcAcuteAngleBonus(angle);

                        // Or there was no stream before
                        double timeDifferenceFactor = DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLastObj.StrainTime * 0.75, osuLastObj.StrainTime * 0.55);

                        streamNerf *= Math.Max(reverseAngleBonus, timeDifferenceFactor);
                    }
                }

                double overlappedNotesWeight = 1;

                if (current.Index > 2)
                {
                    var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);

                    double currOverlapness = getOverlapness((OsuHitObject)osuCurrObj.BaseObject, (OsuHitObject)osuLastObj.BaseObject);
                    double prevOverlapness = getOverlapness((OsuHitObject)osuCurrObj.BaseObject, (OsuHitObject)osuLastLastObj.BaseObject);

                    overlappedNotesWeight = Math.Max(currOverlapness, prevOverlapness);
                }

                angleBonus = Math.Max(angleChangeBonus, acuteAngleBonus) * (1 - streamNerf) * overlappedNotesWeight;
            }

            double velocityChangeBonus = Math.Abs(prevVelocity - currVelocity);

            flowDifficulty += angleBonus + velocityChangeBonus;
            flowDifficulty *= flowMultiplier;

            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderBonus = osuLastObj.TravelDistance / osuLastObj.TravelTime;
                flowDifficulty += sliderBonus * AimEvaluator.SLIDER_MULTIPLIER;
            }

            return flowDifficulty;
        }

        private static double getOverlapness(OsuHitObject a, OsuHitObject b)
        {
            var vector = a.StackedPosition - b.StackedPosition;
            double diameterSqr = 16 * a.Radius * a.Radius;
            double distanceSqr = vector.LengthSquared;
            return Math.Clamp((distanceSqr / diameterSqr - 0.5) * 2, 0, 1);
        }

        public static double CalculateFlowAcuteAngleBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            if (osuCurrObj.Angle == null)
                return 0;

            double currAngle = osuCurrObj.Angle.Value;

            double acuteAngleBonus = AimEvaluator.CalcAcuteAngleBonus(currAngle);
            acuteAngleBonus *= Math.Min(osuCurrObj.LazyJumpDistance, osuLastObj.LazyJumpDistance) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime);

            return acuteAngleBonus;
        }

        public static double CalculateFlowAngleChangeBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            if (osuCurrObj.AngleSigned == null || osuLastObj.AngleSigned == null)
                return 0;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            double currAngle = osuCurrObj.AngleSigned.Value;
            double lastAngle = osuLastObj.AngleSigned.Value;

            double minVelocity = Math.Min(currVelocity, prevVelocity);
            double angleChangeBonus = Math.Pow(Math.Sin((currAngle - lastAngle) / 2), 2) * minVelocity;

            return angleChangeBonus;
        }

        public static double CalculateFlowVelocityChangeBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            return Math.Abs(prevVelocity - currVelocity);
        }
    }
}
