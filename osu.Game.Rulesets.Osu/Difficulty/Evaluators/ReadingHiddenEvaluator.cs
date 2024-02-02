// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    // Class for HD calc. Split because there are a lot of things in HD calc.
    public static class ReadingHiddenEvaluator
    {
        private const double reading_window_size = 3000;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var currObj = (OsuDifficultyHitObject)current;

            double density = 0;
            double densityAnglesNerf = -2; // we have threshold of 2, so 2 or same angles won't be punished

            OsuDifficultyHitObject? prevObj0 = null;
            OsuDifficultyHitObject? prevObj1 = null;
            OsuDifficultyHitObject? prevObj2 = null;

            double prevConstantAngle = 0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj).Reverse())
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                // For HD: it's not subtracting anything cuz it's multiplied by the aim difficulty anyways.
                // loopDifficulty *= logistic((loopObj.MinimumJumpDistance) / 15);

                // Reduce density bonus for this object if they're too apart in time
                // Nerf starts on 1500ms and reaches maximum (*=0) on 3000ms
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                if (prevObj0.IsNull())
                {
                    prevObj0 = loopObj;
                    continue;
                }

                // HD-exclusive burst nerf

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

                if (loopObj.Angle.IsNotNull() && prevObj0.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(prevObj0.Angle.Value - loopObj.Angle.Value);

                    // Nerf alternating angles case
                    if (prevObj1.IsNotNull() && prevObj2.IsNotNull() && prevObj1.Angle.IsNotNull() && prevObj2.Angle.IsNotNull())
                    {
                        // Normalized difference
                        double angleDifference1 = Math.Abs(prevObj1.Angle.Value - loopObj.Angle.Value) / Math.PI;
                        double angleDifference2 = Math.Abs(prevObj2.Angle.Value - prevObj0.Angle.Value) / Math.PI;

                        // Will be close to 1 if angleDifference1 and angleDifference2 was both close to 0
                        double alternatingFactor = Math.Pow((1 - angleDifference1) * (1 - angleDifference2), 2);

                        // Be sure to nerf only same rhythms
                        double rhythmFactor = 1 - getRhythmDifference(loopObj.StrainTime, prevObj0.StrainTime); // 0 on different rhythm, 1 on same rhythm
                        rhythmFactor *= 1 - getRhythmDifference(prevObj0.StrainTime, prevObj1.StrainTime);
                        rhythmFactor *= 1 - getRhythmDifference(prevObj1.StrainTime, prevObj2.StrainTime);

                        double acuteAngleFactor = 1 - Math.Min(loopObj.Angle.Value, prevObj0.Angle.Value) / Math.PI;

                        double prevAngleAdjust = Math.Max(angleDifference - angleDifference1, 0);

                        prevAngleAdjust *= alternatingFactor; // Nerf if alternating
                        prevAngleAdjust *= rhythmFactor; // Nerf if same rhythms
                        prevAngleAdjust *= acuteAngleFactor;

                        angleDifference -= prevAngleAdjust;
                    }

                    // Reduce angles nerf if objects are too apart in time
                    // Angle nerf is starting being reduced from 200ms (150BPM jump) and it reduced to 0 on 2000ms
                    double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - 200) / (2000 - 200), 0, 1);

                    // Current angle nerf. Angle difference less than 15 degrees is considered the same
                    double currConstantAngle = Math.Cos(4 * Math.Min(Math.PI / 12, angleDifference)) * longIntervalFactor;

                    // Apply the nerf only when it's repeated
                    double currentAngleNerf = Math.Min(currConstantAngle, prevConstantAngle);

                    densityAnglesNerf += Math.Min(currentAngleNerf, loopDifficulty);
                    prevConstantAngle = currConstantAngle;
                }

                prevObj2 = prevObj1;
                prevObj1 = prevObj0;
                prevObj0 = loopObj;
            }

            // Apply angles nerf
            density -= Math.Max(0, densityAnglesNerf);

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

        //public static double EvaluateHiddenDifficultyOfOld(DifficultyHitObject current)
        //{
        //    var currObj = (OsuDifficultyHitObject)current;

        //    double aimDifficulty = AimEvaluator.EvaluateDifficultyOf(current, false);

        //    double timeSpentInvisible = getDurationSpentInvisible(currObj) / currObj.ClockRate;

        //    double density = 1 + Math.Max(0, CalculateDenstityOf(currObj) - 1);

        //    double timeDifficultyFactor = density / 1000;
        //    timeDifficultyFactor *= getConstantAngleNerfFactor(currObj);

        //    double visibleObjectFactor = Math.Clamp(retrieveCurrentVisibleObjects(currObj).Count - 2, 0, 15);

        //    double hdDifficulty = visibleObjectFactor * timeSpentInvisible * timeDifficultyFactor +
        //                    (6 + visibleObjectFactor) * aimDifficulty;

        //    hdDifficulty *= 0.95 + 0.15 * EvaluateInpredictabilityOf(current); // Max multiplier is 1.1

        //    return hdDifficulty;
        //}

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

        //private static double getDurationSpentInvisible(OsuDifficultyHitObject current)
        //{
        //    var baseObject = (OsuHitObject)current.BaseObject;

        //    double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeIn;
        //    double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

        //    return (fadeOutStartTime + fadeOutDuration) - (baseObject.StartTime - baseObject.TimePreempt);
        //}

        //private static List<OsuDifficultyHitObject> retrieveCurrentVisibleObjects(OsuDifficultyHitObject current)
        //{
        //    List<OsuDifficultyHitObject> objects = new List<OsuDifficultyHitObject>();

        //    for (int i = 0; i < current.Count; i++)
        //    {
        //        OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Next(i);

        //        if (hitObject.IsNull() ||
        //            (hitObject.StartTime - current.StartTime) > reading_window_size ||
        //            current.StartTime < hitObject.StartTime - hitObject.Preempt)
        //            break;

        //        objects.Add(hitObject);
        //    }

        //    return objects;
        //}

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        private static double getRhythmDifference(double t1, double t2) => 1 - Math.Min(t1, t2) / Math.Max(t1, t2);
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));
    }
}
