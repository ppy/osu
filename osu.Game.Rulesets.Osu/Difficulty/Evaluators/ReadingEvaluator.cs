// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
        private const double reading_window_size = 3000;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, double clockRate, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double constantAngleNerfFactor = getConstantAngleNerfFactor(currObj);
            double angularVelocityFactor = getAngularVelocityFactor(currObj, (OsuDifficultyHitObject)currObj.Previous(0));

            double pastObjectDifficultyInfluence = 1.0;

            if (currObj.BaseObject is Slider currSlider)
                // Longer sliders are inherently denser objects
                pastObjectDifficultyInfluence += 2 * Math.Log10(Math.Max(1, currSlider.Velocity * currSlider.SpanDuration / currSlider.Radius));

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= DifficultyCalculationUtils.Logistic(-(loopObj.LazyJumpDistance - 75) / 15);

                double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / clockRate;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                loopDifficulty *= timeNerfFactor;
                pastObjectDifficultyInfluence += loopDifficulty;
            }

            double preemptDifficulty = 0.0;

            double currPreempt = currObj.Preempt; // Approach rate in milliseconds

            if (currPreempt < 500)
            {
                preemptDifficulty += Math.Pow(500 - currPreempt, 2.5) / 120000;

                // Nerf preempt on most comfortable densities
                // https://www.desmos.com/calculator/31mrv4rlfh
                double densityDifficulty = 1 + DifficultyCalculationUtils.BellCurve(retrievePastVisibleObjects(currObj).Count(), 2, 1.5, 3.0);
                preemptDifficulty *= constantAngleNerfFactor * angularVelocityFactor / densityDifficulty;
            }

            double hiddenDifficulty = 0.0;

            if (hidden)
            {
                double timeSpentInvisible = getDurationSpentInvisible(currObj) / clockRate;

                // Nerf hidden difficulty less the more past object difficulty you have
                double timeDifficultyFactor = 9000 / pastObjectDifficultyInfluence;

                // Cap objects because after a certain point hidden density is mainly memory
                double visibleObjectFactor = Math.Min(retrieveCurrentVisibleObjects(currObj).Count, 8);

                hiddenDifficulty += visibleObjectFactor * timeSpentInvisible / timeDifficultyFactor;

                hiddenDifficulty *= constantAngleNerfFactor * angularVelocityFactor;

                // Buff perfect stacks
                hiddenDifficulty += currObj.LazyJumpDistance == 0 ? 2 : 0;
            }

            // Award only denser than average maps
            double noteDensityDifficulty = Math.Max(0, pastObjectDifficultyInfluence - 2.7);
            noteDensityDifficulty *= constantAngleNerfFactor * angularVelocityFactor;

            double difficulty = preemptDifficulty + hiddenDifficulty + noteDensityDifficulty;

            return difficulty;
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

        // Returns a list of objects that are visible on screen at
        // the point in time at which the current object needs to be clicked.
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
                    constantAngleCount += Math.Cos(2 * Math.Min(Math.PI / 4, angleDifference)) * longIntervalFactor;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;
            }

            return Math.Min(1, 2 / constantAngleCount);
        }

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        private static double getAngularVelocityFactor(OsuDifficultyHitObject current, OsuDifficultyHitObject previous)
        {
            if (current.Angle.HasValue &&
                previous?.Angle != null &&
                Math.Abs(current.DeltaTime - previous.DeltaTime) < 10)
            {
                double angleDifference = Math.Abs(current.Angle.Value - previous.Angle.Value);
                double angleDifferenceAdjusted = Math.Sin(angleDifference / 2) * 180.0;
                double angularVelocity = angleDifferenceAdjusted * (current.LazyJumpDistance / current.StrainTime);
                double angularVelocityBonus = Math.Max(0.0, Math.Pow(angularVelocity, 0.4) - 1.0);
                return angularVelocityBonus * 0.35;
            }

            return current.LazyJumpDistance / current.StrainTime;
        }
    }
}
