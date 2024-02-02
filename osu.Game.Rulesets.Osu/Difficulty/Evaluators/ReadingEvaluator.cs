// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    // Main class with some util functions
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;

        private const double overlap_multiplier = 0.8;

        public static double CalculateDenstityOf(OsuDifficultyHitObject currObj)
        {
            double pastObjectDifficultyInfluence = 0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= logistic((loopObj.MinimumJumpDistance - 60) / 10);

                //double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / clockRateEstimate;
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                pastObjectDifficultyInfluence += loopDifficulty;
            }

            return pastObjectDifficultyInfluence;
        }

        public static double CalculateOverlapDifficultyOf(OsuDifficultyHitObject currObj)
        {
            double screenOverlapDifficulty = 0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double lastOverlapness = 0;
                foreach (var overlapObj in loopObj.OverlapObjects)
                {
                    if (overlapObj.HitObject.StartTime + overlapObj.HitObject.Preempt > currObj.StartTime) break;
                    lastOverlapness = overlapObj.Overlapness;
                }
                screenOverlapDifficulty += lastOverlapness;
            }

            return screenOverlapDifficulty;
        }
        public static double EvaluateDensityDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;

            double pastObjectDifficultyInfluence = CalculateDenstityOf(currObj);
            double screenOverlapDifficulty = CalculateOverlapDifficultyOf(currObj);

            double difficulty = Math.Pow(4 * Math.Log(Math.Max(1, pastObjectDifficultyInfluence)), 2.3);

            screenOverlapDifficulty = Math.Max(0, screenOverlapDifficulty - 0.5); // make overlap value =1 cost significantly less

            double overlapBonus = overlap_multiplier * screenOverlapDifficulty * difficulty;

            difficulty *= getConstantAngleNerfFactor(currObj);
            difficulty += overlapBonus;

            //difficulty *= 1 + overlap_multiplier * screenOverlapDifficulty;

            return difficulty;
        }

        // Returns value from 0 to 1, where 0 is very predictable and 1 is very unpredictable
        public static double EvaluateInpredictabilityOf(DifficultyHitObject current)
        {
            // make the sum equal to 1
            const double velocity_change_part = 0.25;
            const double angle_change_part = 0.45;
            const double rhythm_change_part = 0.3;

            if (current.BaseObject is Spinner || current.Index == 0 || current.Previous(0).BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)current.Previous(0);

            // Rhythm difference punishment for velocity and angle bonuses
            double rhythmSimilarity = 1 - getRhythmDifference(osuCurrObj.StrainTime, osuLastObj.StrainTime);

            // Make differentiation going from 1/4 to 1/2 and bigger difference
            // To 1/3 to 1/2 and smaller difference
            rhythmSimilarity = Math.Clamp(rhythmSimilarity, 0.5, 0.75);
            rhythmSimilarity = 4 * (rhythmSimilarity - 0.5);

            double velocityChangeBonus = getVelocityChangeFactor(osuCurrObj, osuLastObj) * rhythmSimilarity;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            double angleChangeBonus = 0;

            if (osuCurrObj.Angle != null && osuLastObj.Angle != null && currVelocity > 0 && prevVelocity > 0)
            {
                angleChangeBonus = Math.Pow(Math.Sin((double)((osuCurrObj.Angle - osuLastObj.Angle) / 2)), 2); // Also stealed from xexxar
                angleChangeBonus *= Math.Min(currVelocity, prevVelocity) / Math.Max(currVelocity, prevVelocity); // Prevent cheesing
            }

            angleChangeBonus *= rhythmSimilarity;

            // This bonus only awards rhythm changes if they're not filled with sliderends
            double rhythmChangeBonus = 0;

            if (current.Index > 1)
            {
                var osuLastLastObj = (OsuDifficultyHitObject)current.Previous(1);

                double currDelta = osuCurrObj.StrainTime;
                double lastDelta = osuLastObj.StrainTime;

                if (osuLastObj.BaseObject is Slider sliderCurr)
                {
                    currDelta -= sliderCurr.Duration / osuCurrObj.ClockRate;
                    currDelta = Math.Max(0, currDelta);
                }

                if (osuLastLastObj.BaseObject is Slider sliderLast)
                {
                    lastDelta -= sliderLast.Duration / osuLastObj.ClockRate;
                    lastDelta = Math.Max(0, lastDelta);
                }

                rhythmChangeBonus = getRhythmDifference(currDelta, lastDelta);
            }

            double result = velocity_change_part * velocityChangeBonus + angle_change_part * angleChangeBonus + rhythm_change_part * rhythmChangeBonus;
            return result;
        }

        private static double getVelocityChangeFactor(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj)
        {
            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            double velocityChangeFactor = 0;

            // https://www.desmos.com/calculator/kqxmqc8pkg
            if (currVelocity > 0 || prevVelocity > 0)
            {
                double velocityChange = Math.Max(0,
                Math.Min(
                    Math.Abs(prevVelocity - currVelocity) - 0.5 * Math.Min(currVelocity, prevVelocity),
                    Math.Max(((OsuHitObject)osuCurrObj.BaseObject).Radius / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), Math.Min(currVelocity, prevVelocity))
                    )); // Stealed from xexxar
                velocityChangeFactor = velocityChange / Math.Max(currVelocity, prevVelocity); // maxiumum is 0.4
                velocityChangeFactor /= 0.4;
            }

            return velocityChangeFactor;
        }

        // Returns a list of objects that are visible on screen at
        // the point in time at which the current object becomes visible.
        private static IEnumerable<OsuDifficultyHitObject> retrievePastVisibleObjects(OsuDifficultyHitObject current)
        {
            for (int i = 0; i < current.Index; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Previous(i);

                if (hitObject.IsNull() ||
                    current.StartTime - hitObject.StartTime > reading_window_size ||
                    hitObject.StartTime < current.StartTime - current.Preempt)
                    break;

                yield return hitObject;
            }
        }

        private static double getConstantAngleNerfFactor(OsuDifficultyHitObject current)
        {
            const double time_limit = 2000;
            const double time_limit_low = 200;

            double constantAngleCount = 0;
            int index = 0;
            double currentTimeGap = 0;

            OsuDifficultyHitObject prevLoopObj = current;

            OsuDifficultyHitObject? prevLoopObj1 = null;
            OsuDifficultyHitObject? prevLoopObj2 = null;

            double prevConstantAngle = 0;

            while (currentTimeGap < time_limit)
            {
                var loopObj = (OsuDifficultyHitObject)current.Previous(index);

                if (loopObj.IsNull())
                    break;

                double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - time_limit_low) / (time_limit - time_limit_low), 0, 1);

                if (loopObj.Angle.IsNotNull() && prevLoopObj.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(prevLoopObj.Angle.Value - loopObj.Angle.Value);

                    // Nerf alternating angles case
                    if (prevLoopObj1.IsNotNull() && prevLoopObj2.IsNotNull() && prevLoopObj1.Angle.IsNotNull() && prevLoopObj2.Angle.IsNotNull())
                    {
                        // Normalized difference
                        double angleDifference1 = Math.Abs(prevLoopObj1.Angle.Value - loopObj.Angle.Value) / Math.PI;
                        double angleDifference2 = Math.Abs(prevLoopObj2.Angle.Value - prevLoopObj.Angle.Value) / Math.PI;

                        // Will be close to 1 if angleDifference1 and angleDifference2 was both close to 0
                        double alternatingFactor = Math.Pow((1 - angleDifference1) * (1 - angleDifference2), 2);

                        // Be sure to nerf only same rhythms
                        double rhythmFactor = 1 - getRhythmDifference(loopObj.StrainTime, prevLoopObj.StrainTime); // 0 on different rhythm, 1 on same rhythm
                        rhythmFactor *= 1 - getRhythmDifference(prevLoopObj.StrainTime, prevLoopObj1.StrainTime);
                        rhythmFactor *= 1 - getRhythmDifference(prevLoopObj1.StrainTime, prevLoopObj2.StrainTime);

                        double acuteAngleFactor = 1 - Math.Min(loopObj.Angle.Value, prevLoopObj.Angle.Value) / Math.PI;

                        double prevAngleAdjust = Math.Max(angleDifference - angleDifference1, 0);

                        prevAngleAdjust *= alternatingFactor; // Nerf if alternating
                        prevAngleAdjust *= rhythmFactor; // Nerf if same rhythms
                        prevAngleAdjust *= acuteAngleFactor;

                        angleDifference -= prevAngleAdjust;
                    }

                    double currConstantAngle = Math.Cos(4 * Math.Min(Math.PI / 8, angleDifference)) * longIntervalFactor;
                    constantAngleCount += Math.Min(currConstantAngle, prevConstantAngle);
                    prevConstantAngle = currConstantAngle;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;

                prevLoopObj2 = prevLoopObj1;
                prevLoopObj1 = prevLoopObj;
                prevLoopObj = loopObj;
            }

            return Math.Pow(Math.Min(1, 2 / constantAngleCount), 2);
        }

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        private static double getRhythmDifference(double t1, double t2) => 1 - Math.Min(t1, t2) / Math.Max(t1, t2);
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));
    }
}
