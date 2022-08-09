// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class CognitionEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;

            List<DifficultyHitObject> pastVisibleObjects = retrievePastVisibleObjects(currObj);
            List<DifficultyHitObject> currentVisibleObjects = retrieveCurrentVisibleObjects(currObj);

            // Rather than note density being the number of on-screen objects visible at the current object,
            // consider it as how many objects the current object has been visible for.
            double noteDensity = 1.0;

            double loopOpacity = 1.0;
            int previousIndex = 0;

            while (loopOpacity > 0)
            {
                var loopObj = (OsuDifficultyHitObject)currObj.Previous(previousIndex);

                if (loopObj.IsNull())
                    break;

                loopOpacity = currObj.OpacityAt(loopObj.StartTime, false);

                if (loopOpacity <= 0)
                    break;

                noteDensity += loopOpacity;
                previousIndex++;
            }

            double noteDensityDifficulty = Math.Pow(Math.Max(0, noteDensity - 2), 2);

            double hiddenDifficulty = 0;

            if (hidden)
            {
                noteDensityDifficulty *= 3.2;

                // Really not sure about this, but without this a lot of normal HD plays become underweight.
                hiddenDifficulty = 7 * currObj.LazyJumpDistance / currObj.StrainTime;
            }

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
        private static List<DifficultyHitObject> retrievePastVisibleObjects(OsuDifficultyHitObject current)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            for (int i = 0; i < current.Index; i++)
            {
                DifficultyHitObject currentObj = current.Previous(i);

                if (current.OpacityAt(currentObj.StartTime, false) <= 0)
                    break;

                objects.Add(currentObj);
            }

            return objects;
        }

        // Returns a list of objects that are visible on screen at
        // the point in time at which the current object needs is clicked.
        private static List<DifficultyHitObject> retrieveCurrentVisibleObjects(OsuDifficultyHitObject current)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            for (int i = 0; i < current.Count; i++)
            {
                OsuDifficultyHitObject currentObj = (OsuDifficultyHitObject)current.Next(i);

                if (currentObj.IsNull() || currentObj.OpacityAt(current.StartTime, false) <= 0)
                    break;

                objects.Add(currentObj);
            }

            return objects;
        }

        private static double logistic(double x) => 1 / (1 + Math.Pow(Math.E, -x));
    }
}
