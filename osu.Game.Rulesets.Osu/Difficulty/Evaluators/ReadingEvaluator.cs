// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;
        private const double hidden_multiplier = 0.6;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, IReadOnlyList<Mod> mods)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;

            double rawDensityDifficulty = 1.0;

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= DifficultyCalculationUtils.Logistic(-(loopObj.MinimumJumpDistance - 80) / 15);

                double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / current.ClockRate;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                rawDensityDifficulty += loopDifficulty;
            }

            double preemptDifficulty = 0.0;

            double currApproachRate = currObj.Preempt; // Approach rate in milliseconds

            if (currApproachRate < 450)
            {
                preemptDifficulty += Math.Pow(450 - currApproachRate, 2.3) / 27000;

                // Buff spacing.
                preemptDifficulty *= currVelocity;

                // Nerf preempt difficulty with density, lower density means more difficulty
                // This is on the basis that in a high density environment you can rely more on patterns and muscle memory
                preemptDifficulty /= rawDensityDifficulty;
            }

            double noteDensityDifficulty = Math.Max(0, rawDensityDifficulty - 2.5); // Density difficulty begins at over 2.5 units

            double hiddenDifficulty = 0;

            if (mods.OfType<OsuModHidden>().Any())
            {
                double timeSpentInvisible = getDurationSpentInvisible(currObj) / current.ClockRate;
                // Nerf hidden difficulty less the more density difficulty you have
                // We stop nerfing at density of 1 because there is a still an inherent hidden difficulty at low density
                double timeDifficultyFactor = noteDensityDifficulty <= 1 ? 400 : 400 / noteDensityDifficulty;

                double visibleObjectFactor = Math.Clamp(retrieveCurrentVisibleObjects(currObj).Count - 2, 0, 15);

                // The longer an object is hidden, the more velocity should matter
                hiddenDifficulty += (visibleObjectFactor + timeSpentInvisible * currVelocity) / timeDifficultyFactor;
            }

            // preemptDifficulty *= 0.2;
            double difficulty = preemptDifficulty + hiddenDifficulty * hidden_multiplier + noteDensityDifficulty;

            difficulty *= getConstantAngleNerfFactor(currObj);

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
    }
}
