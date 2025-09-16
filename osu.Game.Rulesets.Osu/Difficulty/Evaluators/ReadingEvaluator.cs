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
        private const double distance_influence_threshold = OsuDifficultyHitObject.NORMALISED_DIAMETER * 1.35; // 1.35 circles distance between centers
        private const double hidden_multiplier = 0.85;
        private const double density_multiplier = 0.8;
        private const double density_difficulty_base = 3.0;
        private const double preempt_balancing_factor = 200000;
        private const double preempt_starting_point = 475; // AR 9.83 in milliseconds

        public static double EvaluateDifficultyOf(int totalObjects, DifficultyHitObject current, double clockRate, double preempt, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double constantAngleNerfFactor = getConstantAngleNerfFactor(currObj);
            double velocity = Math.Max(1, currObj.MinimumJumpDistance / currObj.AdjustedDeltaTime); // Only allow velocity to buff

            double pastObjectDifficultyInfluence = 0.0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj, preempt))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= DifficultyCalculationUtils.Smootherstep(loopObj.LazyJumpDistance, 15, distance_influence_threshold);

                // Account less for objects close to the max reading window
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                loopDifficulty *= timeNerfFactor;
                pastObjectDifficultyInfluence += loopDifficulty;
            }

            // Value higher note densities exponentially
            double noteDensityDifficulty = Math.Pow(pastObjectDifficultyInfluence, 1.45) * 0.9 * constantAngleNerfFactor * velocity;

            // Award only denser than average maps.
            noteDensityDifficulty = Math.Max(0, noteDensityDifficulty - density_difficulty_base);

            // Apply a soft cap to general density reading to account for partial memorization
            noteDensityDifficulty = Math.Pow(noteDensityDifficulty, 0.8) * density_multiplier;

            double hiddenDifficulty = 0.0;

            if (hidden)
            {
                double timeSpentInvisible = getDurationSpentInvisible(currObj) / clockRate;

                // Value time spent invisible exponentially
                double timeSpentInvisibleFactor = Math.Pow(timeSpentInvisible, 2.1) * 0.0001;

                // Buff current note if upcoming notes are dense
                // This is on the basis that part of hidden difficulty is the uncertainty of the current cursor position in relation to future notes
                double futureObjectDifficultyInfluence = retrieveCurrentVisibleObjects(totalObjects, currObj, preempt);

                // Account for both past and current densities
                double densityFactor = Math.Pow(Math.Max(1, futureObjectDifficultyInfluence + pastObjectDifficultyInfluence - 2), 2.2) * 3.1;

                hiddenDifficulty += (timeSpentInvisibleFactor + densityFactor) * constantAngleNerfFactor * velocity * 0.007;

                // Apply a soft cap to general HD reading to account for partial memorization
                hiddenDifficulty = Math.Pow(hiddenDifficulty, 0.65) * hidden_multiplier;

                var previousObj = currObj.Previous(0);
                // Buff perfect stacks only if current note is completely invisible at the time you click the previous note.
                if (currObj.LazyJumpDistance == 0 && currObj.OpacityAt(previousObj.BaseObject.StartTime + preempt, hidden) == 0 && previousObj.StartTime + preempt > currObj.StartTime)
                    hiddenDifficulty += hidden_multiplier * 1303 / Math.Pow(currObj.AdjustedDeltaTime, 1.5); // Perfect stacks are harder the less time between notes
            }

            double preemptDifficulty = 0.0;

            // Arbitrary curve for the base value preempt difficulty should have as approach rate increases.
            // https://www.desmos.com/calculator/c175335a71
            preemptDifficulty += Math.Pow((preempt_starting_point - preempt + Math.Abs(preempt - preempt_starting_point)) / 2, 2.5) / preempt_balancing_factor;

            preemptDifficulty *= constantAngleNerfFactor * velocity;

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

        // Returns the density of objects visible at the point in time the current object needs to be clicked.
        private static double retrieveCurrentVisibleObjects(int totalObjects, OsuDifficultyHitObject current, double preempt)
        {
            double visibleObjectCount = 0;

            for (int i = 0; i < totalObjects; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Next(i);

                if (hitObject.IsNull() ||
                    hitObject.StartTime - current.StartTime > reading_window_size ||
                    current.StartTime + preempt < hitObject.StartTime) // Object not visible at the time current object needs to be clicked.
                    break;

                double timeBetweenCurrAndLoopObj = hitObject.StartTime - current.StartTime;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                visibleObjectCount += hitObject.OpacityAt(current.BaseObject.StartTime, false) * timeNerfFactor;
            }

            return visibleObjectCount;
        }

        // Returns the amount of time a note spends invisible with the hidden mod at the current approach rate.
        private static double getDurationSpentInvisible(OsuDifficultyHitObject current)
        {
            var baseObject = (OsuHitObject)current.BaseObject;

            double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeIn;
            double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

            return (fadeOutStartTime + fadeOutDuration) - (baseObject.StartTime - baseObject.TimePreempt);
        }

        // Returns a factor of how often the current object's angle has been repeated in a certain time frame.
        // It does this by checking the difference in angle between current and past objects and sums them based on a range of similarity.
        // https://www.desmos.com/calculator/eb057a4822
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

                // Account less for objects that are close to the time limit.
                double longIntervalFactor = Math.Clamp(1 - (loopObj.AdjustedDeltaTime - time_limit_low) / (time_limit - time_limit_low), 0, 1);

                if (loopObj.Angle.IsNotNull() && current.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(current.Angle.Value - loopObj.Angle.Value);
                    constantAngleCount += Math.Cos(3 * Math.Min(Math.PI / 6, angleDifference)) * longIntervalFactor;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;
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
