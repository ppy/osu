// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class FlowAimEvaluator
    {
        /// <summary>
        /// Evaluates difficulty of "flow aim" - aiming pattern where player doesn't stop their cursor on every object and instead "flows" through them.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool withSliderTravelDistance)
        {
            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous();

            if (current.BaseObject is Spinner || current.Index <= 1 || osuLastObj.BaseObject is Spinner)
                return 0;

            var osuNextObj = (OsuDifficultyHitObject?)current.Next();
            var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);

            double currDistance = withSliderTravelDistance ? osuCurrObj.LazyJumpDistance : osuCurrObj.JumpDistance;
            double prevDistance = withSliderTravelDistance ? osuLastObj.LazyJumpDistance : osuLastObj.JumpDistance;

            double currVelocity = calculateCurrentVelocity(
                osuCurrObj,
                osuLastObj,
                currDistance,
                withSliderTravelDistance);

            double prevVelocity = prevDistance / osuLastObj.AdjustedDeltaTime;

            double flowDifficulty = currVelocity;

            // Apply high circle size bonus to the base velocity.
            // We use reduced CS bonus here because the bonus was made for an evaluator with a different d/t scaling
            flowDifficulty *= Math.Sqrt(osuCurrObj.SmallCircleBonus);

            flowDifficulty *= calculateRhythmChangeBonus(osuCurrObj, osuLastObj);
            flowDifficulty *= calculateAngularVelocityBonus(osuCurrObj, osuLastObj);

            if (osuNextObj != null)
            {
                flowDifficulty += calculateAcuteAngleBonus(
                    osuCurrObj,
                    osuLastObj,
                    osuLastLastObj,
                    osuNextObj,
                    currVelocity);
            }

            flowDifficulty += calculateVelocityChangeBonus(
                osuCurrObj,
                osuLastObj,
                currVelocity,
                prevVelocity,
                currDistance,
                calculateOverlapWeight(osuCurrObj, osuLastObj, osuLastLastObj),
                withSliderTravelDistance);

            if (osuCurrObj.BaseObject is Slider && withSliderTravelDistance)
            {
                flowDifficulty += calculateSliderBonus(osuCurrObj);
            }

            // Final velocity is being raised to a power because flow difficulty scales harder with both high distance and time, and we want to account for that
            flowDifficulty = DiffUtils.Pow(flowDifficulty, 1.45);

            // Reduce difficulty for low spacing since spacing below radius is always to be flowed
            return flowDifficulty * DiffUtils.Smootherstep(currDistance, 0, OsuDifficultyHitObject.NORMALISED_RADIUS);
        }

        private static double calculateRhythmChangeBonus(OsuDifficultyHitObject current, OsuDifficultyHitObject last)
        {
            const double maximum_rhythm_change_bonus = 0.1;

            double bonus = DiffUtils.Pow(
                (Math.Max(current.AdjustedDeltaTime, last.AdjustedDeltaTime) - Math.Min(current.AdjustedDeltaTime, last.AdjustedDeltaTime)) / 50,
                4);

            return 1 + Math.Min(maximum_rhythm_change_bonus, bonus);
        }

        /// <summary>
        /// Scales flow difficulty by angular velocity.
        /// This nerfs consistent angles whilst buffing "erratic" flow.
        /// </summary>
        private static double calculateAngularVelocityBonus(OsuDifficultyHitObject current, OsuDifficultyHitObject last)
        {
            if (current.Angle == null || last.Angle == null)
                return 1;

            double angleDifference = Math.Abs(current.Angle.Value - last.Angle.Value);
            double angleDifferenceAdjusted = Math.Sin(angleDifference / 2) * 180.0;
            double angularVelocity = angleDifferenceAdjusted / (current.AdjustedDeltaTime * 0.1);

            return 0.8 + Math.Sqrt(angularVelocity / 270.0);
        }

        private static double calculateAcuteAngleBonus(
            OsuDifficultyHitObject current,
            OsuDifficultyHitObject last,
            OsuDifficultyHitObject lastLast,
            OsuDifficultyHitObject next,
            double currVelocity)
        {
            const double acute_angle_multiplier = 1.3;

            if (current.Angle == null || next.Angle == null)
                return 0;

            double currAcuteness = AngleUtils.CalculateAcuteness(current.Angle.Value);
            double nextAcuteness = AngleUtils.CalculateAcuteness(next.Angle.Value);

            double acuteness;
            double overlapWeight;

            // We want to evaluate flow turns at the center point of the actual turn, but curr.Angle is a prev2-prev-curr angle.
            // The issue with changing that to prev-curr-next is that we might evaluate the second note of a flow pattern as snap if prev is acute.
            // With min(curr,next) the evaluation (assuming acute affects snap/flow probability enough) behaves roughly like this:
            //
            //    flow (prev-curr-next and prev2-prev-curr evaluates as wide)
            //     🡓🡓
            //     ooo 🡐 flow (prev2-prev-curr evaluates as wide)
            //      /
            //   ooo 🡐 snap (prev2-prev-curr evaluates as acute)
            //   🡑🡑
            //  flow (prev-curr-next evaluates as wide)
            //
            //
            //  flow (prev-curr-next and prev2-prev-curr evaluates as wide)
            //   🡓🡓
            //   ooo 🡐 flow (prev-curr-next and prev2-prev-curr evaluates as wide)
            //      \
            //     ooo 🡐 snap (prev-curr-next evaluates as acute as the center point of the turn)
            //     🡑🡑
            //    flow (prev-curr-next evaluates as wide)
            //
            // In both examples the first object in a flow pattern is evaluated as acute (likely snap) and the rest are wide (likely flow).
            if (currAcuteness < nextAcuteness)
            {
                acuteness = currAcuteness;
                overlapWeight = calculateOverlapWeight(current, last, lastLast);
            }
            else
            {
                acuteness = nextAcuteness;
                overlapWeight = calculateOverlapWeight(next, current, last);
            }

            return currVelocity * acuteness * overlapWeight * acute_angle_multiplier;
        }

        private static double calculateVelocityChangeBonus(OsuDifficultyHitObject current, OsuDifficultyHitObject previous, double currVelocity, double prevVelocity,
                                                           double currDistance, double overlappedNotesWeight, bool withSliderTravelDistance)
        {
            const double velocity_change_multiplier = 0.55;

            if (Math.Max(prevVelocity, currVelocity) == 0)
                return 0;

            if (withSliderTravelDistance)
                currVelocity = currDistance / current.AdjustedDeltaTime;

            // Scale with ratio of difference compared to 0.5 * max dist.
            double distRatio = DiffUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity) / Math.Max(prevVelocity, currVelocity), 0, 1);

            // Reward for % distance up to 125 / strainTime for overlaps where velocity is still changing.
            double overlapVelocityBuff = Math.Min(OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.25 / Math.Min(current.AdjustedDeltaTime, previous.AdjustedDeltaTime),
                Math.Abs(prevVelocity - currVelocity));

            return overlapVelocityBuff * distRatio * overlappedNotesWeight * velocity_change_multiplier;
        }

        private static double calculateSliderBonus(OsuDifficultyHitObject current)
            => current.TravelDistance / current.TravelTime;

        private static double calculateCurrentVelocity(OsuDifficultyHitObject current, OsuDifficultyHitObject previous, double currDistance, bool withSliderTravelDistance)
        {
            double currVelocity = currDistance / current.AdjustedDeltaTime;

            // If the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (previous.BaseObject is Slider && withSliderTravelDistance)
            {
                double sliderDistance = previous.LazyTravelDistance + current.LazyJumpDistance;
                currVelocity = Math.Max(currVelocity, sliderDistance / current.AdjustedDeltaTime);
            }

            return currVelocity;
        }

        private static double calculateOverlapWeight(OsuDifficultyHitObject current, OsuDifficultyHitObject previous, OsuDifficultyHitObject lastLast)
        {
            double o1 = calculateOverlapFactor(current, previous);
            double o2 = calculateOverlapFactor(current, lastLast);
            double o3 = calculateOverlapFactor(previous, lastLast);

            return 1 - o1 * o2 * o3;
        }

        private static double calculateOverlapFactor(OsuDifficultyHitObject first, OsuDifficultyHitObject second)
        {
            var firstBase = (OsuHitObject)first.BaseObject;
            var secondBase = (OsuHitObject)second.BaseObject;
            double objectRadius = firstBase.Radius;

            double distance = Vector2.Distance(firstBase.StackedPosition, secondBase.StackedPosition);
            return Math.Clamp(1 - DiffUtils.Pow(Math.Max(distance - objectRadius, 0) / objectRadius, 2), 0, 1);
        }
    }
}
