// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000; // 3 seconds
        private const double distance_influence_threshold = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.5; // 1.5 circles distance between centers
        private const double hidden_multiplier = 0.28;
        private const double density_multiplier = 2.4;
        private const double density_difficulty_base = 2.5;
        private const double preempt_balancing_factor = 140000;
        private const double preempt_starting_point = 500; // AR 9.66 in milliseconds
        private const double minimum_angle_relevancy_time = 2000; // 2 seconds
        private const double maximum_angle_relevancy_time = 200;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            var nextObj = (OsuDifficultyHitObject)current.Next(0);

            double velocity = Math.Max(1, currObj.LazyJumpDistance / currObj.AdjustedDeltaTime); // Only allow velocity to buff

            double currentVisibleObjectDensity = retrieveCurrentVisibleObjectDensity(currObj);
            double pastObjectDifficultyInfluence = getPastObjectDifficultyInfluence(currObj);

            double constantAngleNerfFactor = getConstantAngleNerfFactor(currObj);

            double noteDensityDifficulty = calculateDensityDifficulty(nextObj, velocity, constantAngleNerfFactor, pastObjectDifficultyInfluence, currentVisibleObjectDensity);

            double hiddenDifficulty = hidden
                ? calculateHiddenDifficulty(currObj, pastObjectDifficultyInfluence, currentVisibleObjectDensity, velocity, constantAngleNerfFactor)
                : 0;

            double preemptDifficulty = calculatePreemptDifficulty(velocity, constantAngleNerfFactor, currObj.Preempt);

            double difficulty = DifficultyCalculationUtils.Norm(1.5, preemptDifficulty, hiddenDifficulty, noteDensityDifficulty);

            return difficulty;
        }

        /// <summary>
        /// Calculates the density difficulty of the current object and how hard it is to aim it because of it based on:
        /// <list type="bullet">
        /// <item><description>cursor velocity to the current object,</description></item>
        /// <item><description>how many times the current object's angle was repeated,</description></item>
        /// <item><description>density of objects visible when the current object appears,</description></item>
        /// <item><description>density of objects visible when the current object needs to be clicked,</description></item>
        /// /// </list>
        /// </summary>
        private static double calculateDensityDifficulty(OsuDifficultyHitObject? nextObj, double velocity, double constantAngleNerfFactor,
                                                         double pastObjectDifficultyInfluence, double currentVisibleObjectDensity)
        {
            // Consider future densities too because it can make the path the cursor takes less clear
            double futureObjectDifficultyInfluence = Math.Sqrt(currentVisibleObjectDensity);

            if (nextObj != null)
            {
                // Reduce difficulty if movement to next object is small
                futureObjectDifficultyInfluence *= DifficultyCalculationUtils.Smootherstep(nextObj.LazyJumpDistance, 15, distance_influence_threshold);
            }

            // Value higher note densities exponentially
            double noteDensityDifficulty = Math.Pow(pastObjectDifficultyInfluence + futureObjectDifficultyInfluence, 1.7) * 0.4 * constantAngleNerfFactor * velocity;

            // Award only denser than average maps.
            noteDensityDifficulty = Math.Max(0, noteDensityDifficulty - density_difficulty_base);

            // Apply a soft cap to general density reading to account for partial memorization
            noteDensityDifficulty = Math.Pow(noteDensityDifficulty, 0.45) * density_multiplier;

            return noteDensityDifficulty;
        }

        /// <summary>
        /// Calculates the difficulty of aiming the current object when the approach rate is very high based on:
        /// <list type="bullet">
        /// <item><description>cursor velocity to the current object,</description></item>
        /// <item><description>how many times the current object's angle was repeated,</description></item>
        /// <item><description>how many milliseconds elapse between the approach circle appearing and touching the inner circle</description></item>
        /// </list>
        /// </summary>
        private static double calculatePreemptDifficulty(double velocity, double constantAngleNerfFactor, double preempt)
        {
            // Arbitrary curve for the base value preempt difficulty should have as approach rate increases.
            // https://www.desmos.com/calculator/c175335a71
            double preemptDifficulty = Math.Pow((preempt_starting_point - preempt + Math.Abs(preempt - preempt_starting_point)) / 2, 2.5) / preempt_balancing_factor;

            preemptDifficulty *= constantAngleNerfFactor * velocity;

            return preemptDifficulty;
        }

        /// <summary>
        /// Calculates the difficulty of aiming the current object when the hidden mod is active based on:
        /// <list type="bullet">
        /// <item><description>cursor velocity to the current object,</description></item>
        /// <item><description>time the current object spends invisible,</description></item>
        /// <item><description>density of objects visible when the current object appears,</description></item>
        /// <item><description>density of objects visible when the current object needs to be clicked,</description></item>
        /// <item><description>how many times the current object's angle was repeated,</description></item>
        /// <item><description>if the current object is perfectly stacked to the previous one</description></item>
        /// </list>
        /// </summary>
        private static double calculateHiddenDifficulty(OsuDifficultyHitObject currObj, double pastObjectDifficultyInfluence, double currentVisibleObjectDensity, double velocity,
                                                        double constantAngleNerfFactor)
        {
            // Higher preempt means that time spent invisible is higher too, we want to reward that
            double preemptFactor = Math.Pow(currObj.Preempt, 2.2) * 0.01;

            // Account for both past and current densities
            double densityFactor = Math.Pow(currentVisibleObjectDensity + pastObjectDifficultyInfluence, 3.3) * 3;

            double hiddenDifficulty = (preemptFactor + densityFactor) * constantAngleNerfFactor * velocity * 0.01;

            // Apply a soft cap to general HD reading to account for partial memorization
            hiddenDifficulty = Math.Pow(hiddenDifficulty, 0.4) * hidden_multiplier;

            var previousObj = (OsuDifficultyHitObject)currObj.Previous(0);

            // Buff perfect stacks only if current note is completely invisible at the time you click the previous note.
            if (currObj.LazyJumpDistance == 0 && currObj.OpacityAt(previousObj.BaseObject.StartTime + previousObj.Preempt, true) == 0 && previousObj.StartTime + previousObj.Preempt > currObj.StartTime)
                hiddenDifficulty += hidden_multiplier * 2500 / Math.Pow(currObj.AdjustedDeltaTime, 1.5); // Perfect stacks are harder the less time between notes

            return hiddenDifficulty;
        }

        private static double getPastObjectDifficultyInfluence(OsuDifficultyHitObject currObj)
        {
            double pastObjectDifficultyInfluence = 0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // When aiming an object small distances mean previous objects may be cheesed, so it doesn't matter whether they were arranged confusingly.
                loopDifficulty *= DifficultyCalculationUtils.Smootherstep(loopObj.LazyJumpDistance, 15, distance_influence_threshold);

                // Account less for objects close to the max reading window
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                loopDifficulty *= timeNerfFactor;
                pastObjectDifficultyInfluence += loopDifficulty;
            }

            return pastObjectDifficultyInfluence;
        }

        // Returns a list of objects that are visible on screen at the point in time the current object becomes visible.
        private static IEnumerable<OsuDifficultyHitObject> retrievePastVisibleObjects(OsuDifficultyHitObject current)
        {
            for (int i = 0; i < current.Index; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Previous(i);

                if (hitObject.IsNull() ||
                    current.StartTime - hitObject.StartTime > reading_window_size ||
                    hitObject.StartTime + hitObject.Preempt < current.StartTime) // Current object not visible at the time object needs to be clicked
                    break;

                yield return hitObject;
            }
        }

        // Returns the density of objects visible at the point in time the current object needs to be clicked capped by the reading window.
        private static double retrieveCurrentVisibleObjectDensity(OsuDifficultyHitObject current)
        {
            double visibleObjectCount = 0;

            OsuDifficultyHitObject? hitObject = (OsuDifficultyHitObject)current.Next(0);

            while (hitObject != null)
            {
                if (hitObject.StartTime - current.StartTime > reading_window_size ||
                    current.StartTime + hitObject.Preempt < hitObject.StartTime) // Object not visible at the time current object needs to be clicked.
                    break;

                double timeBetweenCurrAndLoopObj = hitObject.StartTime - current.StartTime;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                visibleObjectCount += hitObject.OpacityAt(current.BaseObject.StartTime, false) * timeNerfFactor;

                hitObject = (OsuDifficultyHitObject?)hitObject.Next(0);
            }

            return visibleObjectCount;
        }

        // Returns a factor of how often the current object's angle has been repeated in a certain time frame.
        // It does this by checking the difference in angle between current and past objects and sums them based on a range of similarity.
        // https://www.desmos.com/calculator/eb057a4822
        private static double getConstantAngleNerfFactor(OsuDifficultyHitObject current)
        {
            double constantAngleCount = 0;
            int index = 0;
            double currentTimeGap = 0;

            OsuDifficultyHitObject loopObjPrev0 = current;
            OsuDifficultyHitObject? loopObjPrev1 = null;
            OsuDifficultyHitObject? loopObjPrev2 = null;

            while (currentTimeGap < minimum_angle_relevancy_time)
            {
                var loopObj = (OsuDifficultyHitObject)current.Previous(index);

                if (loopObj.IsNull())
                    break;

                // Account less for objects that are close to the time limit.
                double longIntervalFactor = 1 - DifficultyCalculationUtils.ReverseLerp(loopObj.AdjustedDeltaTime, maximum_angle_relevancy_time, minimum_angle_relevancy_time);

                if (loopObj.Angle.IsNotNull() && current.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(current.Angle.Value - loopObj.Angle.Value);
                    double angleDifferenceAlternating = Math.PI;

                    if (loopObjPrev0.Angle != null && loopObjPrev1?.Angle != null && loopObjPrev2?.Angle != null)
                    {
                        angleDifferenceAlternating = Math.Abs(loopObjPrev1.Angle.Value - loopObj.Angle.Value);
                        angleDifferenceAlternating += Math.Abs(loopObjPrev2.Angle.Value - loopObjPrev0.Angle.Value);

                        double weight = 1.0;

                        // Be sure that one of the angles is very sharp, when other is wide
                        weight *= DifficultyCalculationUtils.ReverseLerp(Math.Min(loopObj.Angle.Value, loopObjPrev0.Angle.Value), 0.1 * Math.PI, 0.02 * Math.PI);
                        weight *= DifficultyCalculationUtils.ReverseLerp(Math.Max(loopObj.Angle.Value, loopObjPrev0.Angle.Value), 0.4 * Math.PI, 0.65 * Math.PI);

                        // Also be sure that spacings are also alternating
                        weight *= DifficultyCalculationUtils.ReverseLerp(loopObj.JumpDistance, loopObjPrev0.JumpDistance * 0.6 + 1, loopObjPrev1.JumpDistance * 0.8 + 1);
                        weight *= DifficultyCalculationUtils.ReverseLerp(loopObj.JumpDistance, loopObjPrev0.JumpDistance * 1.4 + 1, loopObjPrev1.JumpDistance * 1.2 + 1);
                        weight *= DifficultyCalculationUtils.ReverseLerp(loopObjPrev1.JumpDistance, loopObjPrev2.JumpDistance * 0.6 + 1, loopObjPrev2.JumpDistance * 0.8 + 1);
                        weight *= DifficultyCalculationUtils.ReverseLerp(loopObjPrev1.JumpDistance, loopObjPrev2.JumpDistance * 1.4 + 1, loopObjPrev2.JumpDistance * 1.2 + 1);

                        // Don't nerf extremely low spaced patterns
                        weight *= DifficultyCalculationUtils.ReverseLerp(Math.Max(loopObj.JumpDistance, loopObjPrev0.JumpDistance), OsuDifficultyHitObject.NORMALISED_RADIUS, OsuDifficultyHitObject.NORMALISED_RADIUS * 3);

                        // Lerp between max angle difference and rescaled alternating difference, with more harsh scaling compared to normal difference
                        angleDifferenceAlternating = double.Lerp(Math.PI, 0.1 * angleDifferenceAlternating, weight);
                    }

                    double stackFactor = DifficultyCalculationUtils.Smootherstep(loopObj.LazyJumpDistance, 0, OsuDifficultyHitObject.NORMALISED_RADIUS);

                    constantAngleCount += Math.Cos(3 * Math.Min(double.DegreesToRadians(30), Math.Min(angleDifference, angleDifferenceAlternating) * stackFactor)) * longIntervalFactor;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;

                loopObjPrev2 = loopObjPrev1;
                loopObjPrev1 = loopObjPrev0;
                loopObjPrev0 = loopObj;
            }

            return Math.Clamp(2 / constantAngleCount, 0.2, 1);
        }

        // Returns a nerfing factor for when objects are very distant in time, affecting reading less.
        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }
    }
}
