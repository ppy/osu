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
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, IReadOnlyList<Mod> mods)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            Vector2 currPosition = currObj.Position;
            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;
            double angleNerfFactor = getConstantAngleNerfFactor(currObj);
            // Console.Out.WriteLine(angleNerfFactor);

            double pastObjectDifficultyInfluence = 1.0;
            double overlapness = 0;
            double rhythmFactor = 1;

            if (currObj.BaseObject is Slider currSlider)
                // Longer sliders are inherently denser objects
                pastObjectDifficultyInfluence += 2 * Math.Log10(Math.Max(1, currSlider.Velocity * currSlider.SpanDuration / currSlider.Radius));

            foreach (var loopObj in retrievePastVisibleObjects(currObj))
            {
                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= DifficultyCalculationUtils.Logistic(-(loopObj.MinimumJumpDistance - 65) / 15);

                var loopLastObj = (OsuDifficultyHitObject)loopObj.Previous(0);

                if (loopLastObj is not null && loopObj.StrainTime > loopLastObj.StrainTime)
                {
                    double rhythmSimilarity = getRhythmDifference(loopObj.StrainTime, loopLastObj.StrainTime);
                    rhythmSimilarity *= Math.Pow(Math.Sin(rhythmSimilarity * 2 * Math.PI), 2);
                    rhythmFactor += rhythmSimilarity;
                }

                var loopObjPosition = loopObj.Position;
                float visibleDistance = ((currPosition - loopObjPosition) / OsuDifficultyHitObject.NORMALISED_DIAMETER).Length;

                overlapness += DifficultyCalculationUtils.Logistic(-(0.5 - visibleDistance) / 0.1) - 0.2;

                if (loopObj.BaseObject is Slider loopSlider)
                    overlapness *= Math.Max(1, Math.Log10(Math.Max(1, loopSlider.Velocity * loopSlider.SpanDuration / loopSlider.Radius)));

                overlapness = Math.Max(0, overlapness);

                // Nerf overlapness if loop object that overlaps current object also overlaps previous loop object
                // This effectively awards overlap bonus only if there are spacing changes between current object and loop object
                // Ignore if current object is a slider because sliders in bursts add difficulty
                var prevLoopObj = (OsuDifficultyHitObject)loopObj.Next(0);
                overlapness *= prevLoopObj.BaseObject.StartTime.Equals(currObj.BaseObject.StartTime) || loopObj.BaseObject is not Slider ? DifficultyCalculationUtils.Logistic(-(prevLoopObj.MinimumJumpDistance - 70) / 15) : 1;

                double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / currObj.ClockRate;
                double timeNerfFactor = getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                loopDifficulty *= timeNerfFactor;
                overlapness *= timeNerfFactor;
                pastObjectDifficultyInfluence += loopDifficulty;
            }

            overlapness *= rhythmFactor;

            double preemptDifficulty = 0.0;

            double currApproachRate = currObj.Preempt; // Approach rate in milliseconds

            if (currApproachRate < 500)
            {
                preemptDifficulty += Math.Pow(500 - currApproachRate, 2.5) / 110000;

                // Nerf preempt on most comfortable densities
                // https://www.desmos.com/calculator/31mrv4rlfh
                double densityDifficulty = 1 + DifficultyCalculationUtils.BellCurve(retrievePastVisibleObjects(currObj).Count(), 2, 1.5, 3.0);
                preemptDifficulty *= currVelocity / densityDifficulty;
                preemptDifficulty *= angleNerfFactor;
                var prevObj = (OsuDifficultyHitObject)currObj.Previous(0);
                double doubletapness = 1 - prevObj.GetDoubletapness(currObj);
                preemptDifficulty *= doubletapness; // Doubletaps raise the density without adding significant reading difficulty
            }

            double hiddenDifficulty = 0.0;

            if (mods.OfType<OsuModHidden>().Any())
            {
                double timeSpentInvisible = getDurationSpentInvisible(currObj) / currObj.ClockRate;

                // Nerf hidden difficulty less the more past object difficulty you have
                double timeDifficultyFactor = 9000 / pastObjectDifficultyInfluence;

                // Cap objects because after a certain point hidden density is mainly memory
                double visibleObjectFactor = Math.Min(retrieveCurrentVisibleObjects(currObj).Count, 8);

                // The longer an object is hidden, the more velocity should matter
                hiddenDifficulty += visibleObjectFactor * timeSpentInvisible * Math.Max(1, currVelocity) / timeDifficultyFactor;

                hiddenDifficulty *= angleNerfFactor;

                // Buff perfect stacks
                hiddenDifficulty += currObj.LazyJumpDistance == 0 ? 1.5 : 0;
            }

            // Award only denser than average maps
            double noteDensityDifficulty = Math.Max(0, pastObjectDifficultyInfluence - 2.7);
            noteDensityDifficulty *= angleNerfFactor;

            double difficulty = preemptDifficulty + hiddenDifficulty + noteDensityDifficulty + overlapness;

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

            return Math.Pow(Math.Min(1, 2 / constantAngleCount), 2);
        }

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        }

        private static double getRhythmDifference(double t1, double t2) => 1 - Math.Min(t1, t2) / Math.Max(t1, t2);
    }
}
