// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using FFmpeg.AutoGen;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class CognitionEvaluator
    {
        private const double cognition_window_size = 2000;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;

            List<OsuDifficultyHitObject> pastVisibleObjects = retrievePastVisibleObjects(currObj);
            List<OsuDifficultyHitObject> currentVisibleObjects = retrieveCurrentVisibleObjects(currObj);

            // Rather than note density being the number of on-screen objects visible at the current object,
            // consider it as how many objects the current object has been visible for.
            double noteDensityDifficulty = 1.0;

            foreach (var loopObj in pastVisibleObjects)
            {
                var prevLoopObj = (OsuDifficultyHitObject)loopObj.Previous(0);

                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= logistic((loopObj.MinimumJumpDistance - 125) / 15);

                // Objects that are arranged in a mostly-linear fashion should be easy to read (such as circles in a stream).
                //if (loopObj.Angle.IsNotNull() && prevLoopObj.Angle.IsNotNull())
                //   loopDifficulty *= 1 - Math.Pow(Math.Sin(0.5 * loopObj.Angle.Value), 5);

                noteDensityDifficulty += loopDifficulty;
            }

            noteDensityDifficulty = Math.Pow(3 * Math.Log(Math.Max(1, noteDensityDifficulty - 1)), 2.3);

            double hiddenDifficulty = 0;

            double preemptDifficulty = 0.0;
            if (currObj.preempt < 400)
                preemptDifficulty += Math.Pow(400 - currObj.preempt, 1.5) / 14;

            // Buff rhythm on high AR.
            preemptDifficulty *= RhythmEvaluator.EvaluateDifficultyOf(current, 30);

            double difficulty = Math.Max(preemptDifficulty, hiddenDifficulty) + noteDensityDifficulty;

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

        private static double logistic(double x) => 1 / (1 + Math.Pow(Math.E, -x));
    }
}
