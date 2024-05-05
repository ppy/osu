// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    // Main class with some util functions
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;

        public static double EvaluateDensityOf(DifficultyHitObject current, bool applyDistanceNerf = true)
        {
            var currObj = (OsuDifficultyHitObject)current;

            double density = 0;
            double densityAnglesNerf = -2; // we have threshold of 2

            // Despite being called prev, it's actually more late in time
            OsuDifficultyHitObject prevObj0 = currObj;

            var readingObjects = currObj.ReadingObjects;
            for (int i = 0; i < readingObjects.Count; i++)
            {
                var loopObj = readingObjects[i].HitObject;

                if (loopObj.Index < 1)
                    continue; // Don't look on the first object of the map

                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                if (applyDistanceNerf) loopDifficulty *= (logistic((loopObj.MinimumJumpDistance - 80) / 10) + 0.2) / 1.2;

                // Reduce density bonus for this object if they're too apart in time
                // Nerf starts on 1500ms and reaches maximum (*=0) on 3000ms
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

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
                double currAngleNerf = (loopObj.AnglePredictability / 2) + 0.5;

                // Apply the nerf only when it's repeated
                double angleNerf = currAngleNerf;

                densityAnglesNerf += angleNerf * loopDifficulty;

                prevObj0 = loopObj;
            }

            // Apply angles nerf
            density -= Math.Max(0, densityAnglesNerf);
            return density;
        }

        // Returns value from 0 to 1, where 0 is very predictable and 1 is very unpredictable
        public static double EvaluateInpredictabilityOf(DifficultyHitObject current)
        {
            // make the sum equal to 1
            const double velocity_change_part = 0.8;
            const double angle_change_part = 0.1;
            const double rhythm_change_part = 0.1;

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
                angleChangeBonus = 1 - osuCurrObj.AnglePredictability;
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

        private static double getTimeNerfFactor(double deltaTime) => Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        private static double getRhythmDifference(double t1, double t2) => 1 - Math.Min(t1, t2) / Math.Max(t1, t2);
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));
    }

    public static class ReadingHiddenEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var currObj = (OsuDifficultyHitObject)current;

            double density = ReadingEvaluator.EvaluateDensityOf(current, false);
            double preempt = currObj.Preempt / 1000;

            double densityFactor = Math.Pow(density / 6.2, 1.5);

            double invisibilityFactor;

            // AR11+DT and faster = 0 HD pp unless density is big
            if (preempt < 0.2) invisibilityFactor = 0;

            // Else accelerating growth until around ART0, then linear, and starting from AR5 is 3 times faster again to buff AR0 +HD
            else invisibilityFactor = Math.Min(Math.Pow(preempt * 2.4 - 0.2, 5), Math.Max(preempt, preempt * 3 - 2.4));


            double hdDifficulty = invisibilityFactor + densityFactor;

            // Scale by inpredictability slightly
            hdDifficulty *= 0.96 + 0.1 * ReadingEvaluator.EvaluateInpredictabilityOf(current); // Max multiplier is 1.1

            return hdDifficulty;
        }
    }
}
