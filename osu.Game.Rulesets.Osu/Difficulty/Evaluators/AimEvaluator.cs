// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class AimEvaluator
    {
        private const double wide_angle_multiplier = 1.5;
        private const double acute_angle_multiplier = 2.24;
        private const double velocity_change_multiplier = 0.74;
        private const double wiggle_multiplier = 1.02;

        public const double SLIDER_MULTIPLIER = 1.27;

        /// <summary>
        /// Evaluates the difficulty of aiming the current object, based on:
        /// <list type="bullet">
        /// <item><description>cursor velocity to the current object,</description></item>
        /// <item><description>angle difficulty,</description></item>
        /// <item><description>sharp velocity increases,</description></item>
        /// <item><description>and slider difficulty.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            if (current.BaseObject is Spinner || current.Index <= 1 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);
            var osuLast1Obj = (OsuDifficultyHitObject)current.Previous(1);

            const int radius = OsuDifficultyHitObject.NORMALISED_RADIUS;
            const int diameter = OsuDifficultyHitObject.NORMALISED_DIAMETER;

            // Additional reward for wide angles being hard to snap on high BPM
            double angleSnapDifficultyBonus = 0;
            double strainTimeThreshold = DifficultyCalculationUtils.BPMToMilliseconds(180, 2);

            if (osuCurrObj.StrainTime < strainTimeThreshold)
            {
                double bpmFactor = Math.Pow((strainTimeThreshold - osuCurrObj.StrainTime) * 0.015, 2.5);

                angleSnapDifficultyBonus = OsuDifficultyHitObject.NORMALISED_DIAMETER * bpmFactor;

                // Shift starting point of "uncomfy" from square to wide-angle patterns if spacing is too big, becvause big spacing is already buffed enough by wide angle bonus
                double highSpacingAdjust = Math.PI / 6;
                highSpacingAdjust *= DifficultyCalculationUtils.ReverseLerp(osuCurrObj.LazyJumpDistance, diameter * 2, diameter * 4);

                angleSnapDifficultyBonus *= DifficultyCalculationUtils.Smoothstep(osuCurrObj.Angle ?? 0, Math.PI / 3 + highSpacingAdjust, Math.PI / 2 + highSpacingAdjust);
            }

            // Adjusting minimal distance of snap evaluator to account for fact that the snapping difficulty itself have it's own difficulty
            double adjustSnapDistance(double currDistance)
            {
                double bpm = DifficultyCalculationUtils.BPMToMilliseconds(osuCurrObj.StrainTime, 2);
                double snapThreshold = diameter * (1 + 1.3 * DifficultyCalculationUtils.ReverseLerp(bpm, 200, 250));

                // Jumps need to have some spacing to be snapped
                double result = currDistance < snapThreshold ? snapThreshold * 0.65 + currDistance * 0.35 : currDistance;

                // Don't buff doubles jumps as you don't snap in this case (except very close to itself doubles, that need to have some distance bonus to be calculated as flow)
                double lowSpacingFactor = DifficultyCalculationUtils.ReverseLerp(currDistance, radius * 2, radius);
                double timeFactor = DifficultyCalculationUtils.ReverseLerpTwoDirectional(osuCurrObj.StrainTime, osuLastObj.StrainTime, 0.75, 0.95);

                // Make nerf much smaller if it's not doubles and time is different
                double notOverlappingAdjust = diameter * 2 * (1 - lowSpacingFactor) * (2 - timeFactor);

                // Don't increase snap distance when previous jump is very big, as it leads to cheese being overrewarded
                double bigDistanceDifferenceFactor = DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, notOverlappingAdjust + diameter, notOverlappingAdjust + diameter * 2);

                // And don't nerf bursts with this
                bigDistanceDifferenceFactor *= DifficultyCalculationUtils.ReverseLerpTwoDirectional(osuCurrObj.StrainTime, osuLastObj.StrainTime, 1.95, 1.5);

                double totalBonus = result + angleSnapDifficultyBonus - currDistance;
                return currDistance + totalBonus * (1 - bigDistanceDifferenceFactor);
            }

            // Calculate the velocity to the current hitobject, which starts with a base distance / time assuming the last object is a hitcircle.
            double currDistance = adjustSnapDistance(osuCurrObj.LazyJumpDistance);
            double currVelocity = currDistance / osuCurrObj.StrainTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLastObj.TravelDistance / osuLastObj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = adjustSnapDistance(osuCurrObj.MinimumJumpDistance) / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            if (osuLast1Obj.BaseObject is Slider && withSliderTravelDistance)
            {
                double travelVelocity = osuLast1Obj.TravelDistance / osuLast1Obj.TravelTime;
                double movementVelocity = osuLastObj.MinimumJumpDistance / osuLastObj.MinimumJumpTime;

                prevVelocity = Math.Max(prevVelocity, movementVelocity + travelVelocity);
            }

            double wideAngleBonus = 0;
            double acuteAngleBonus = 0;
            double sliderBonus = 0;
            double velocityChangeBonus = 0;
            double wiggleBonus = 0;

            double aimStrain = currVelocity; // Start strain with regular velocity.

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null)
            {
                // Buff wide only if rhythms are the same
                double maxStrainTime = Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime);
                double minStrainTime = Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime);
                double differentRhythmMultiplier = DifficultyCalculationUtils.Smoothstep(maxStrainTime, 1.25 * minStrainTime, minStrainTime);

                // Buff high bpm and wiggles only if rhythm is the same or getting slower (burst -> jump)
                double fasterRhythmMultiplier = DifficultyCalculationUtils.Smoothstep(osuLastObj.StrainTime, osuCurrObj.StrainTime * 1.25, osuCurrObj.StrainTime);

                double currAngle = osuCurrObj.Angle.Value;
                double lastAngle = osuLastObj.Angle.Value;
                double last1Angle = osuLast1Obj.Angle ?? Math.PI;

                // Rewarding angles, take the smaller velocity as base.
                double acuteVelocityBase = Math.Min(currVelocity, prevVelocity);

                // If previous object was a burst - use current velocity instead of min
                acuteVelocityBase = double.Lerp(acuteVelocityBase, currVelocity,
                    Math.Pow(DifficultyCalculationUtils.Smoothstep(osuCurrObj.StrainTime, osuLastObj.StrainTime, osuLastObj.StrainTime * 1.75), 2));

                double wideVelocityBase = Math.Min(currDistance / osuCurrObj.StrainTime, prevVelocity); // Don't reward wide angle bonus to sliders

                double velocityThreshold = diameter * 2.3 / osuCurrObj.StrainTime;

                if (wideVelocityBase > velocityThreshold) // Nerf high spaced squares to compensate total square buff
                {
                    wideVelocityBase = velocityThreshold + 0.4 * (wideVelocityBase - velocityThreshold);
                }

                // Potentially wide should also use fasterRhythmMultiplier, but there are some unwanted buffs alongside the wanted ones
                wideAngleBonus = differentRhythmMultiplier * CalcWideAngleBonus(currAngle);
                acuteAngleBonus = fasterRhythmMultiplier * CalcAcuteAngleBonus(currAngle);

                // Penalize angle repetition. Ideally this thing should be removed, but it breaks balance so I've just make it weaker by taking min between angles
                double wideAngleRepetitionNerf = Math.Min(wideAngleBonus, Math.Pow(CalcWideAngleBonus(Math.Min(lastAngle, last1Angle)), 3));
                wideAngleBonus *= 1 - wideAngleRepetitionNerf;

                double acuteAngleRepetitionNerf = Math.Pow(CalcAcuteAngleBonus(lastAngle), 3);
                acuteAngleBonus *= 0.08 + 0.65 * (1 - Math.Min(acuteAngleBonus, acuteAngleRepetitionNerf));

                // Apply full wide angle bonus for distance more than one diameter
                wideAngleBonus *= wideVelocityBase * DifficultyCalculationUtils.Smootherstep(osuCurrObj.LazyJumpDistance, 0, diameter);

                // Apply acute angle bonus for BPM above 300 1/2 and distance more than one diameter
                acuteAngleBonus *= acuteVelocityBase *
                                    DifficultyCalculationUtils.Smootherstep(DifficultyCalculationUtils.MillisecondsToBPM(osuCurrObj.StrainTime, 2), 300, 400);

                // Apply wiggle bonus for jumps that are [radius, 3*diameter] in distance, with < 110 angle
                // https://www.desmos.com/calculator/dp0v0nvowc
                wiggleBonus = acuteVelocityBase
                                * fasterRhythmMultiplier
                                * DifficultyCalculationUtils.Smootherstep(currDistance, radius, diameter)
                                * Math.Pow(DifficultyCalculationUtils.ReverseLerp(currDistance, diameter * 3, diameter), 1.8)
                                * DifficultyCalculationUtils.Smootherstep(currAngle, double.DegreesToRadians(110), double.DegreesToRadians(60))
                                * DifficultyCalculationUtils.Smootherstep(osuLastObj.LazyJumpDistance, radius, diameter)
                                * Math.Pow(DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, diameter * 3, diameter), 1.8)
                                * DifficultyCalculationUtils.Smootherstep(lastAngle, double.DegreesToRadians(110), double.DegreesToRadians(60));
            }

            // We want to use the average velocity over the whole object when awarding differences, not the individual jump and slider path velocities.
            prevVelocity = (osuLastObj.LazyJumpDistance + osuLast1Obj.TravelDistance) / osuLastObj.StrainTime;
            currVelocity = (osuCurrObj.LazyJumpDistance + osuLastObj.TravelDistance) / osuCurrObj.StrainTime;

            if (Math.Max(prevVelocity, currVelocity) != 0)
            {
                // Scale with ratio of difference compared to 0.5 * max dist.
                double distRatio = Math.Pow(Math.Sin(Math.PI / 2 * Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity)), 2);

                // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
                double overlapVelocityBuff = Math.Min(diameter * 1.25 / Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime), Math.Abs(prevVelocity - currVelocity));

                velocityChangeBonus = overlapVelocityBuff * distRatio;

                // Penalize for rhythm changes.
                double rhythmPenalty = Math.Pow(Math.Min(osuCurrObj.StrainTime, osuLastObj.StrainTime) / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), 2);
                velocityChangeBonus *= rhythmPenalty;

                var osuLast2Obj = (OsuDifficultyHitObject)current.Previous(2);
                double prev1Distance = osuLast1Obj.LazyJumpDistance;
                double prev2Distance = osuLast2Obj?.LazyJumpDistance ?? 0;

                // If previously there was slow flow pattern - sudden velocity change is much easier because you could flow faster to give yourself more time
                // Add radius to account for distance potenitally being very small
                double distanceSimilarityFactor = DifficultyCalculationUtils.ReverseLerp(prev1Distance + radius, (prev2Distance + radius) * 0.8, (prev2Distance + radius) * 0.95);
                double distanceFactor = 0.5 + 0.5 * DifficultyCalculationUtils.ReverseLerp(Math.Max(prev1Distance, prev2Distance), diameter * 1.5, diameter * 0.75);

                // We don't nerf more snappy patterns with this as it's much more difficult to snap faster compared to flow faster
                double angleFactor = DifficultyCalculationUtils.Smoothstep(Math.Max(osuLast1Obj.Angle ?? 0, osuLast2Obj?.Angle ?? 0), Math.PI * 0.55, Math.PI * 0.75);

                velocityChangeBonus *= 1 - distanceSimilarityFactor * distanceFactor * angleFactor * rhythmPenalty;

                // Decrease buff large jumps leading into very small jumps to compensate the fact that smaller jumps are buffed by minimal snap distance
                // Use 2 different curves for doubles and microjumps here for better balancing
                double doublesNerf = 0.8 * DifficultyCalculationUtils.ReverseLerp(osuCurrObj.LazyJumpDistance, diameter, diameter * 3) * DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, diameter, radius);
                double microJumpsNerf = 0.75 * DifficultyCalculationUtils.ReverseLerp(osuCurrObj.LazyJumpDistance, diameter * 2.5, diameter * 5) * DifficultyCalculationUtils.ReverseLerp(osuLastObj.LazyJumpDistance, diameter * 2, diameter);
                velocityChangeBonus *= 1 - Math.Max(doublesNerf, microJumpsNerf) * rhythmPenalty;
            }

            if (osuLastObj.BaseObject is Slider)
            {
                // Reward sliders based on velocity.
                sliderBonus = osuLastObj.TravelDistance / osuLastObj.TravelTime;
            }

            aimStrain += wiggleBonus * wiggle_multiplier;

            // Add in acute angle bonus or wide angle bonus + velocity change bonus, whichever is larger.
            aimStrain += Math.Max(acuteAngleBonus * acute_angle_multiplier, wideAngleBonus * wide_angle_multiplier + velocityChangeBonus * velocity_change_multiplier);

            // Add in additional slider velocity bonus.
            if (withSliderTravelDistance)
                aimStrain += sliderBonus * SLIDER_MULTIPLIER;

            return aimStrain;
        }

        public static double CalcWideAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(40), double.DegreesToRadians(140));
        public static double CalcAcuteAngleBonus(double angle) => DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(140), double.DegreesToRadians(40));
    }
}
