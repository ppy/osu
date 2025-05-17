// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000; // 3 seconds

        public static double EvaluateDifficultyOf(int totalObjects, DifficultyHitObject current, double clockRate, double preempt, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double constantAngleNerfFactor = getConstantAngleNerfFactor(currObj);
            double angularVelocityFactor = getAngularVelocityFactor(currObj);

            double pastObjectDifficultyInfluence = 1.0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj, preempt))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                // https://www.desmos.com/calculator/6sgz6j5zb1
                loopDifficulty *= DifficultyCalculationUtils.Logistic(-(loopObj.LazyJumpDistance - 75) / 15);

                // Account less for objects close to the max reading window
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                loopDifficulty *= timeNerfFactor;
                pastObjectDifficultyInfluence += loopDifficulty;
            }

            // Award only denser than average maps
            double noteDensityDifficulty = Math.Max(0, pastObjectDifficultyInfluence - 2.7);

            noteDensityDifficulty *= constantAngleNerfFactor * angularVelocityFactor;

            double hiddenDifficulty = 0.0;

            if (hidden)
            {
                double timeSpentInvisible = getDurationSpentInvisible(currObj) / clockRate;

                // Nerf extremely high times as you begin to rely more on memory the longer a note is invisible
                double timeSpentInvisibleFactor = Math.Min(timeSpentInvisible, 1000) + (timeSpentInvisible > 1000 ? 2000 * Math.Log10(timeSpentInvisible / 1000) : 0);

                // Cap objects because after a certain point hidden density is mainly memory
                double visibleObjectFactor = Math.Min(getCurrentVisibleObjectCount(totalObjects, currObj, preempt), 8);

                hiddenDifficulty += visibleObjectFactor * timeSpentInvisibleFactor * pastObjectDifficultyInfluence / 9000;

                hiddenDifficulty *= constantAngleNerfFactor * angularVelocityFactor;

                // Buff perfect stacks only if current note is completely invisible at the time you click the previous note
                var previousObj = currObj.Previous(0);
                hiddenDifficulty += currObj.LazyJumpDistance == 0 &&
                                    currObj.OpacityAt(previousObj.BaseObject.StartTime + preempt, hidden) == 0 &&
                                    previousObj.StartTime + preempt > currObj.StartTime
                    ? timeSpentInvisibleFactor / 200.0
                    : 0;
            }

            double preemptDifficulty = 0.0;

            // Arbitrary curve for the base value preempt difficulty should have as approach rate increases
            // https://www.desmos.com/calculator/hd9dojqyt2
            preemptDifficulty += preempt > 500 ? 0 : Math.Pow(500 - preempt, 2.3) / 75000;

            preemptDifficulty *= constantAngleNerfFactor * angularVelocityFactor;

            double difficulty = preemptDifficulty + hiddenDifficulty + noteDensityDifficulty;

            return difficulty;
        }

        // Returns a list of objects that are visible on screen at the point in time the current object becomes visible.
        private static IEnumerable<OsuDifficultyHitObject> retrievePastVisibleObjects(OsuDifficultyHitObject current, double preempt)
        {
            for (int i = 0; i < current.Index; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Previous(i);

                if (hitObject.IsNull() ||
                    current.StartTime - hitObject.StartTime > reading_window_size ||
                    hitObject.StartTime + preempt < current.StartTime) // Current object not visible at the time object needs to be clicked
                    break;

                yield return hitObject;
            }
        }

        // Returns the amount of objects visible at the point in time the current object needs to be clicked
        private static int getCurrentVisibleObjectCount(int totalObjects, OsuDifficultyHitObject current, double preempt)
        {
            int visibleObjectCount = 0;

            for (int i = 0; i < totalObjects; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Next(i);

                if (hitObject.IsNull() ||
                    hitObject.StartTime - current.StartTime > reading_window_size ||
                    current.StartTime + preempt < hitObject.StartTime) // Object not visible at the time current object needs to be clicked
                    break;

                visibleObjectCount += 1;
            }

            return visibleObjectCount;
        }

        // Returns the amount of time a note spends invisible with the hidden mod at the current approach rate
        private static double getDurationSpentInvisible(OsuDifficultyHitObject current)
        {
            var baseObject = (OsuHitObject)current.BaseObject;

            double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeIn;
            double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

            return (fadeOutStartTime + fadeOutDuration) - (baseObject.StartTime - baseObject.TimePreempt);
        }

        // Returns a factor of how often the current object's angle has been repeated in a certain time frame
        // It does this by checking the difference in angle between current and past objects and sums them based on a range of similarity
        // https://www.desmos.com/calculator/91acokynyf
        private static double getConstantAngleNerfFactor(OsuDifficultyHitObject current)
        {
            const double time_limit = 2000; // 2 seconds
            const double time_limit_low = 200;

            double constantAngleCount = 0;
            int index = 0;
            double currentTimeGap = 0;

            while (currentTimeGap < time_limit)
            {
                var loopObj = (OsuDifficultyHitObject)current.Previous(index);

                if (loopObj.IsNull())
                    break;

                // Account less for objects that are close to the time limit
                double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - time_limit_low) / (time_limit - time_limit_low), 0, 1);

                if (loopObj.Angle.IsNotNull() && current.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(current.Angle.Value - loopObj.Angle.Value);
                    constantAngleCount += Math.Cos(2 * Math.Min(Math.PI / 4, angleDifference)) * longIntervalFactor;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;
            }

            return Math.Min(1, 2 / constantAngleCount);
        }

        // Returns a nerfing factor for when objects are very distant in time, affecting reading less
        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        // Returns the velocity of going from the previous object to the current scaled by the difference in their angles
        // Includes a targeted nerf for cases where objects go back and forth through a middle point
        // https://www.desmos.com/calculator/5vwzshwz7t
        private static double getAngularVelocityFactor(OsuDifficultyHitObject current)
        {
            var previous = (OsuDifficultyHitObject)current.Previous(0);
            var previous2 = (OsuDifficultyHitObject)current.Previous(2);

            if (!current.Angle.HasValue ||
                previous?.Angle == null ||
                !(Math.Abs(current.DeltaTime - previous.DeltaTime) < 10))
            {
                return current.MinimumJumpDistance / current.StrainTime; // Return unscaled velocity if conditions aren't met
            }

            double angleDifference = Math.Abs(current.Angle.Value - previous.Angle.Value);
            double angleDifferenceAdjusted = 0.1 + Math.Sin(angleDifference / 2) * 180.0;
            double angularVelocity = angleDifferenceAdjusted * (current.MinimumJumpDistance / current.StrainTime);
            double angularVelocityBonus = Math.Max(0.0, Math.Pow(angularVelocity, 0.4) - 1.0) * 0.35;

            if (previous2 == null) return angularVelocityBonus;
            // If objects just go back and forth through a middle point - don't give as much bonus
            // Use Previous(2) and Previous(0) because angles calculation is done prevprev-prev-curr, so any object's angle's center point is always the previous object
            var lastBaseObject = (OsuHitObject)previous.BaseObject;
            var last2BaseObject = (OsuHitObject)previous2.BaseObject;

            float distance = (last2BaseObject.StackedPosition - lastBaseObject.StackedPosition).Length;

            if (distance < 1)
            {
                return angularVelocityBonus * (1 - 0.35 * (1 - distance));
            }

            return angularVelocityBonus;
        }
    }
}
