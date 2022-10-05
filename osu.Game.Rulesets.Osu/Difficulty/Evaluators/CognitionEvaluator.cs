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
    public static class CognitionEvaluator
    {
        private const double cognition_window_size = 3000;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            var prevObj = (OsuDifficultyHitObject)current.Previous(0);

            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;

            // Maybe I should just pass in clockrate...
            var clockRateEstimate = current.BaseObject.StartTime / currObj.StartTime;

            List<OsuDifficultyHitObject> pastVisibleObjects = retrievePastVisibleObjects(currObj);
            //List<OsuDifficultyHitObject> currentVisibleObjects = retrieveCurrentVisibleObjects(currObj);

            // Rather than note density being the number of on-screen objects visible at the current object,
            // consider it as how many objects the current object has been visible for.
            double noteDensityDifficulty = 1.0;

            double pastObjectDifficultyInfluence = 1.0;

            foreach (var loopObj in pastVisibleObjects)
            {
                var prevLoopObj = loopObj.Previous(0) as OsuDifficultyHitObject;

                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= logistic((loopObj.MinimumJumpDistance - 90) / 15);

                double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / clockRateEstimate;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                pastObjectDifficultyInfluence += loopDifficulty;
            }

            noteDensityDifficulty = Math.Pow(3 * Math.Log(Math.Max(1, pastObjectDifficultyInfluence - 1)), 2.3);

            // Objects that are arranged in a mostly-linear fashion should be easy to read (such as circles in a stream).
            if (currObj.Angle.IsNotNull() && prevObj.IsNotNull())
            {
                double prevVelocity = prevObj.LazyJumpDistance / prevObj.StrainTime;
                double velocityDifference = Math.Clamp(Math.Abs(currVelocity - prevVelocity), 0, 1);
                noteDensityDifficulty *= 1 - velocityDifference * Math.Pow(Math.Sin(0.5 * currObj.Angle.Value), 5);
            }

            double hiddenDifficulty = 0;

            if (hidden)
            {
                var timeSpentInvisible = getDurationSpentInvisible(currObj) / clockRateEstimate;
                var isRhythmChange = (currObj.StrainTime - prevObj.StrainTime < 5);

                var timeDifficultyFactor = 800 / pastObjectDifficultyInfluence;

                hiddenDifficulty += Math.Pow(7 * timeSpentInvisible / timeDifficultyFactor, 1);

                if (isRhythmChange)
                    hiddenDifficulty *= 1.1;

                hiddenDifficulty += 2 * currVelocity;
            }

            double preemptDifficulty = 0.0;

            if (currObj.preempt < 400)
            {
                preemptDifficulty += Math.Pow(400 - currObj.preempt, 1.5) / (10 + (currObj.StrainTime * 0.05));

                // Buff spacing.
                preemptDifficulty *= 1 + 0.4 * currVelocity;

                // Buff rhythm.
                preemptDifficulty *= Math.Max(1, RhythmEvaluator.EvaluateDifficultyOf(current, 30) - 0.1);

                // Buff small circles.
                // Very arbitrary, but lets assume CS5 is when AR11 becomes more uncomfortable.
                // This is likely going to need adjustments in the future as player meta develops.
                preemptDifficulty *= 1 + Math.Max((30 - ((OsuHitObject)currObj.BaseObject).Radius) / 20, 0);

                // Nerf repeated angles.
                if (current.Index > 1)
                {
                    var prevPrevObj = (OsuDifficultyHitObject)current.Previous(1);

                    if (currObj.Angle != null && prevObj.Angle != null)
                    {
                        preemptDifficulty *= getAngleDifferenceNerfFactor(Math.Abs(currObj.Angle.Value - prevObj.Angle.Value));
                    }

                    if (currObj.Angle != null && prevPrevObj.Angle != null)
                    {
                        preemptDifficulty *= getAngleDifferenceNerfFactor(Math.Abs(currObj.Angle.Value - prevPrevObj.Angle.Value));
                    }
                }

                // Nerf constant rhythm.
                preemptDifficulty *= getConstantRhythmNerfFactor(currObj);
            }

            double difficulty = Math.Max(preemptDifficulty, hiddenDifficulty) + noteDensityDifficulty;

            // While there is slider leniency...
            if (currObj.BaseObject is Slider)
                difficulty *= 0.2;

            return difficulty;
        }

        // Returns a list of objects that are visible on screen at
        // the point in time at which the current object becomes visible.
        private static List<OsuDifficultyHitObject> retrievePastVisibleObjects(OsuDifficultyHitObject current)
        {
            List<OsuDifficultyHitObject> objects = new List<OsuDifficultyHitObject>();

            for (int i = 0; i < current.Index; i++)
            {
                OsuDifficultyHitObject loopObj = (OsuDifficultyHitObject)current.Previous(i);

                if (loopObj.IsNull() || current.StartTime - loopObj.StartTime > cognition_window_size)
                    break;

                objects.Add(loopObj);
            }

            return objects;
        }

        // Returns a list of objects that are visible on screen at
        // the point in time at which the current object needs is clicked.
        private static List<OsuDifficultyHitObject> retrieveCurrentVisibleObjects(OsuDifficultyHitObject current)
        {
            List<OsuDifficultyHitObject> objects = new List<OsuDifficultyHitObject>();

            for (int i = 0; i < current.Count; i++)
            {
                OsuDifficultyHitObject loopObj = (OsuDifficultyHitObject)current.Next(i);

                if (loopObj.IsNull() || (loopObj.StartTime - current.StartTime) > cognition_window_size)
                    break;

                objects.Add(loopObj);
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

        private static double getConstantRhythmNerfFactor(OsuDifficultyHitObject current)
        {
            // Studies [citation needed] suggest that 33bpm is where humans stop interpreting notes as a part of a beat,
            // instead interpreting them as individual events. We're gonna use this to both lessen the nerf of this factor,
            // as well as using it as a convenient limit for how back in time we're gonna look for the calculation.
            const double time_limit = 1800;
            const double time_limit_low = 500;

            double constantRhythmCount = 0;

            int index = 0;
            double currentTimeGap = 0;

            while (currentTimeGap < time_limit)
            {
                var loopObj = (OsuDifficultyHitObject)current.Previous(index);

                if (loopObj.IsNull())
                    break;

                double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - time_limit_low) / (time_limit - time_limit_low), 0, 1);

                if (Math.Abs(current.StrainTime - loopObj.StrainTime) < 10) // constant rhythm, o-o-o-o
                    constantRhythmCount += 1.0 * longIntervalFactor;
                else if (Math.Abs(current.StrainTime - loopObj.StrainTime * 2) < 10) // speed up rhythm, o---o-o
                    constantRhythmCount += 0.33 * longIntervalFactor;
                else if (Math.Abs(current.StrainTime * 2 - loopObj.StrainTime) < 10) // slow down rhythm, o-o---o
                    constantRhythmCount += 0.33 * longIntervalFactor;
                else
                    break;

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;
            }

            double difficulty = Math.Pow(Math.Min(1, 2 / constantRhythmCount), 2);

            return difficulty;
        }

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - (deltaTime / 1500), 0, 1);
        }

        private static double getAngleDifferenceNerfFactor(double angleDifference) => 1 - 0.5 * Math.Cos(1 * Math.Min(Math.PI / 2, angleDifference));

        private static double logistic(double x) => 1 / (1 + Math.Pow(Math.E, -x));
    }
}
