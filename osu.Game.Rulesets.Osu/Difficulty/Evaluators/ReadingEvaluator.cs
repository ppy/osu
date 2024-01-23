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
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;

        private const double overlap_multiplier = 0.8;

        public static double EvaluateDensityDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;

            double pastObjectDifficultyInfluence = 0;
            double screenOverlapDifficulty = 0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= logistic((loopObj.MinimumJumpDistance - 90) / 15);

                //double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / clockRateEstimate;
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                pastObjectDifficultyInfluence += loopDifficulty;

                double lastOverlapness = 0;
                foreach (var overlapObj in loopObj.OverlapObjects)
                {
                    if (overlapObj.HitObject.StartTime + overlapObj.HitObject.Preempt > currObj.StartTime) break;
                    lastOverlapness = overlapObj.Overlapness;
                }
                screenOverlapDifficulty += lastOverlapness;
            }

            //if (screenOverlapDifficulty > 0)
            //{
            //    Console.WriteLine($"Object {currObj.StartTime}, overlapness = {screenOverlapDifficulty:0.##}");
            //}

            double difficulty = Math.Pow(4 * Math.Log(Math.Max(1, pastObjectDifficultyInfluence)), 2.3);

            screenOverlapDifficulty = Math.Max(0, screenOverlapDifficulty - 0.5); // make overlap value =1 cost significantly less
            difficulty *= getConstantAngleNerfFactor(currObj);

            difficulty *= 1 + overlap_multiplier * screenOverlapDifficulty;

            // Console.WriteLine($"Object {currObj.StartTime}, {hiddenDifficulty:0.##} + {noteDensityDifficulty:0.##} + {overlapDifficulty:0.##}");

            return difficulty;
        }

        public static double EvaluateHighARDifficultyOf(DifficultyHitObject current, bool applyFollowLineAdjust = false)
        {
            // follow lines make high AR easier, so apply nerf if object isn't new combo
            // double adjustedApproachTime = osuCurrObj.Preempt;
            // if (applyFollowLineAdjust) adjustedApproachTime += Math.Max(0, (osuCurrObj.FollowLineTime - 200) / 25);

            var currObj = (OsuDifficultyHitObject)current;

            return highArCurve(currObj.Preempt);
        }

        public static double EvaluateHiddenDifficultyOf(DifficultyHitObject current)
        {
            var currObj = (OsuDifficultyHitObject)current;

            // Maybe I should just pass in clockrate...
            double clockRateEstimate = current.BaseObject.StartTime / currObj.StartTime;
            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;

            double hdDifficulty = 0;

            double timeSpentInvisible = getDurationSpentInvisible(currObj) / clockRateEstimate;

            // Apollo version
            // double timeDifficultyFactor = pastObjectDifficultyInfluence / 1000;

            double visibleObjectFactor = Math.Clamp(retrieveCurrentVisibleObjects(currObj).Count - 2, 0, 15);

            hdDifficulty += Math.Pow(visibleObjectFactor * timeSpentInvisible, 1) +
                            (8 + visibleObjectFactor) * currVelocity;

            return hdDifficulty;
        }

        public static double EvaluatePredictabilityOf(DifficultyHitObject current)
        {
            return 0;
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

        private static List<OsuDifficultyHitObject> retrieveCurrentVisibleObjects(OsuDifficultyHitObject current)
        {
            List<OsuDifficultyHitObject> objects = new List<OsuDifficultyHitObject>();

            for (int i = 0; i < current.Count; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Next(i);

                if (hitObject.IsNull() ||
                    (hitObject.StartTime - current.StartTime) > reading_window_size ||
                    current.StartTime < hitObject.StartTime - hitObject.Preempt)
                    break;

                objects.Add(hitObject);
            }

            return objects;
        }

        private static double getDurationSpentInvisible(OsuDifficultyHitObject current)
        {
            var baseObject = (OsuHitObject)current.BaseObject;

            double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeIn;
            double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

            return (fadeOutStartTime + fadeOutDuration) - (baseObject.StartTime - baseObject.TimePreempt);
        }

        private static double getConstantAngleNerfFactor(OsuDifficultyHitObject current)
        {
            const double time_limit = 2000;
            const double time_limit_low = 200;

            double constantAngleCount = 0;
            int index = 0;
            double currentTimeGap = 0;

            while (currentTimeGap < time_limit)
            {
                var loopObj = (OsuDifficultyHitObject)current.Previous(index);

                if (loopObj.IsNull())
                    break;

                double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - time_limit_low) / (time_limit - time_limit_low), 0, 1);

                if (loopObj.Angle.IsNotNull() && current.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(current.Angle.Value - loopObj.Angle.Value);
                    constantAngleCount += Math.Cos(4 * Math.Min(Math.PI / 8, angleDifference)) * longIntervalFactor;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;
            }

            return Math.Pow(Math.Min(1, 2 / constantAngleCount), 2);
        }
        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        // https://www.desmos.com/calculator/hbj7swzlth
        private static double highArCurve(double preempt)
        {
            double value = Math.Pow(4, 3 - 0.01 * preempt); // 1 for 300ms, 0.25 for 400ms, 0.0625 for 500ms
            value = softmin(value, 2, 1.7); // use softmin to achieve full-memory cap, 2 times more than AR11 (300ms)
            return value;
        }
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));

        // We are using mutiply and divide instead of add and subtract, so values won't be negative
        // https://www.desmos.com/calculator/fv5xerwpd2
        private static double softmin(double a, double b, double power = Math.E) => a * b / Math.Log(Math.Pow(power, a) + Math.Pow(power, b), power);
    }
}
