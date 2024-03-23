// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    // Main class with some util functions
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;

        private const double overlap_multiplier = 2;

        public static double EvaluateDensityOf(DifficultyHitObject current, bool applyDistanceNerf = true)
        {
            var currObj = (OsuDifficultyHitObject)current;
            double density = 0;
            double densityAnglesNerf = -2.5; // we have threshold of 2.5

            OsuDifficultyHitObject? prevObj0 = null;
            OsuDifficultyHitObject? prevObj1 = null;
            OsuDifficultyHitObject? prevObj2 = null;

            double prevAngleNerf = 1;

            foreach (var loopObj in retrievePastVisibleObjects(currObj).Reverse())
            {
                if (loopObj.Index < 1)
                    continue; // Don't look on the first object of the map

                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                if (applyDistanceNerf) loopDifficulty *= (logistic((loopObj.MinimumJumpDistance - 60) / 10) + 0.2) / 1.2;

                // Reduce density bonus for this object if they're too apart in time
                // Nerf starts on 1500ms and reaches maximum (*=0) on 3000ms
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                if (prevObj0.IsNull())
                    prevObj0 = (OsuDifficultyHitObject)loopObj.Previous(0);

                if (prevObj1.IsNull())
                    prevObj1 = (OsuDifficultyHitObject?)loopObj.Previous(1);

                if (prevObj2.IsNull())
                    prevObj2 = (OsuDifficultyHitObject?)loopObj.Previous(2);

                // Only if next object is slower, representing break from many notes in a row
                if (loopObj.StrainTime > prevObj0.StrainTime)
                {
                    // Get rhythm similarity: 1 on same rhythms, 0.5 on 1/4 to 1/2
                    double rhythmSimilarity = 1 - getRhythmDifference(loopObj.StrainTime, prevObj0.StrainTime);

                    // Make differentiation going from 1/4 to 1/2 and bigger difference
                    // To 1/3 to 1/2 and smaller difference
                    rhythmSimilarity = Math.Clamp(rhythmSimilarity, 0.5, 0.75);
                    rhythmSimilarity = 4 * (rhythmSimilarity - 0.5);

                    // Reduce density for this objects if rhythms are different
                    loopDifficulty *= rhythmSimilarity;
                }

                density += loopDifficulty;

                // Angles nerf

                if (loopObj.Angle.IsNotNull() && prevObj0.IsNotNull() && prevObj0.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(prevObj0.Angle.Value - loopObj.Angle.Value);

                    // Assume that very low spacing difference means that angles don't matter
                    if (prevObj0.LazyJumpDistance < OsuDifficultyHitObject.NORMALISED_RADIUS)
                        angleDifference *= Math.Pow(prevObj0.LazyJumpDistance / OsuDifficultyHitObject.NORMALISED_RADIUS, 2);
                    if (loopObj.LazyJumpDistance < OsuDifficultyHitObject.NORMALISED_RADIUS)
                        angleDifference *= Math.Pow(loopObj.LazyJumpDistance / OsuDifficultyHitObject.NORMALISED_RADIUS, 2);

                    // assume worst-case if no angles
                    double angleDifference1 = 0;
                    double angleDifference2 = 0;

                    // Nerf alternating angles case
                    if (prevObj1.IsNotNull() && prevObj2.IsNotNull() && prevObj1.Angle.IsNotNull() && prevObj2.Angle.IsNotNull())
                    {
                        // Normalized difference
                        angleDifference1 = Math.Abs(prevObj1.Angle.Value - loopObj.Angle.Value) / Math.PI;
                        angleDifference2 = Math.Abs(prevObj2.Angle.Value - prevObj0.Angle.Value) / Math.PI;
                    }

                    // Will be close to 1 if angleDifference1 and angleDifference2 was both close to 0
                    double alternatingFactor = Math.Pow((1 - angleDifference1) * (1 - angleDifference2), 2);

                    // Be sure to nerf only same rhythms
                    double rhythmFactor = 1 - getRhythmDifference(loopObj.StrainTime, prevObj0.StrainTime); // 0 on different rhythm, 1 on same rhythm

                    if (prevObj1.IsNotNull())
                        rhythmFactor *= 1 - getRhythmDifference(prevObj0.StrainTime, prevObj1.StrainTime);
                    if (prevObj1.IsNotNull() && prevObj2.IsNotNull())
                        rhythmFactor *= 1 - getRhythmDifference(prevObj1.StrainTime, prevObj2.StrainTime);

                    // double acuteAngleFactor = 1 - Math.Min(loopObj.Angle.Value, prevObj0.Angle.Value) / Math.PI;

                    double prevAngleAdjust = Math.Max(angleDifference - angleDifference1, 0);

                    prevAngleAdjust *= alternatingFactor; // Nerf if alternating
                    prevAngleAdjust *= rhythmFactor; // Nerf if same rhythms
                    // prevAngleAdjust *= acuteAngleFactor; // no longer needed?

                    angleDifference -= prevAngleAdjust;

                    // Reduce angles nerf if objects are too apart in time
                    // Angle nerf is starting being reduced from 200ms (150BPM jump) and it reduced to 0 on 2000ms
                    double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - 200) / (2000 - 200), 0, 1);

                    // Current angle nerf. Angle difference less than 15 degrees is considered the same
                    double currAngleNerf = Math.Cos(4 * Math.Min(Math.PI / 12, angleDifference)) * longIntervalFactor;

                    // Apply the nerf only when it's repeated
                    double angleNerf = Math.Min(currAngleNerf, prevAngleNerf);

                    densityAnglesNerf += Math.Min(angleNerf, loopDifficulty);
                    prevAngleNerf = currAngleNerf;
                }
                else // Assume worst-case if no angles
                {
                    densityAnglesNerf += loopDifficulty;
                }

                prevObj2 = prevObj1;
                prevObj1 = prevObj0;
                prevObj0 = loopObj;
            }

            // Apply angles nerf
            density -= Math.Max(0, densityAnglesNerf);
            return density;
        }

        public static double EvaluateOverlapDifficultyOf(DifficultyHitObject current)
        {
            var currObj = (OsuDifficultyHitObject)current;
            double screenOverlapDifficulty = 0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double lastOverlapness = 0;
                foreach (var overlapObj in loopObj.OverlapObjects)
                {
                    if (overlapObj.HitObject.StartTime + overlapObj.HitObject.Preempt >= currObj.StartTime) break;
                    lastOverlapness = overlapObj.Overlapness;
                }
                screenOverlapDifficulty += lastOverlapness;
            }

            return overlap_multiplier * Math.Max(0, screenOverlapDifficulty - 0.7);
        }
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            double difficulty = Math.Pow(4 * Math.Log(Math.Max(1, EvaluateDensityOf(current))), 2.5);

            double overlapBonus = EvaluateOverlapDifficultyOf(current) * difficulty;
            difficulty += overlapBonus;

            return difficulty;
        }

        public static double EvaluateAimingDensityFactorOf(DifficultyHitObject current)
        {
            double difficulty = EvaluateDensityOf(current);

            double overlapBonus = EvaluateOverlapDifficultyOf(current) * difficulty;
            difficulty += overlapBonus;

            return Math.Max(0, Math.Pow(difficulty, 1.5) - 1);
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

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        private static double getRhythmDifference(double t1, double t2) => 1 - Math.Min(t1, t2) / Math.Max(t1, t2);
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));
    }

    public static class ReadingHiddenEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var currObj = (OsuDifficultyHitObject)current;

            double density = ReadingEvaluator.EvaluateDensityOf(current, false);

            // Consider that density matters only starting from 3rd note on the screen
            double densityFactor = Math.Max(0, density - 1) / 4;

            // This is kinda wrong cuz it returns value bigger than preempt
            // double timeSpentInvisible = getDurationSpentInvisible(currObj) / 1000 / currObj.ClockRate;

            // The closer timeSpentInvisible is to 0 -> the less difference there are between NM and HD
            // So we will reduce base according to this
            // It will be 0.354 on AR11 value
            double invisibilityFactor = logistic(currObj.Preempt / 120 - 4);

            double hdDifficulty = invisibilityFactor + densityFactor;

            // Scale by inpredictability slightly
            hdDifficulty *= 0.95 + 0.15 * ReadingEvaluator.EvaluateInpredictabilityOf(current); // Max multiplier is 1.1

            return hdDifficulty;
        }
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));
    }
}
