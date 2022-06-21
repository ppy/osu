// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
            if (current.BaseObject is Spinner)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            double noteDensity = 1.0;

            double difficulty = 0.0;

            // This loop sucks so much lol.
            // Will be replaced in conjuction with the "objects with current visible" and the "currently visible objects" lists
            // Also variable names like opacity and note density don't seem accurate anymore :face_with_monocole:...
            for (int i = 0; i < 100; i++)
            {
                if (currObj.Next(i + 1).IsNull())
                    break;

                var currLoopObj = (OsuDifficultyHitObject)currObj.Next(i);
                var nextLoopObj = (OsuDifficultyHitObject)currObj.Next(i + 1);

                double opacity = currLoopObj.OpacityAt(currObj.BaseObject.StartTime, false);

                if (opacity == 0)
                    break;

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                opacity *= logistic((currLoopObj.MinimumJumpDistance - 100) / 15);

                // Objects that are arranged in a mostly-linear fashion should be easy to read (such as circles in a stream).
                if (nextLoopObj.Angle.IsNotNull())
                    opacity *= 1 - Math.Pow(Math.Sin(0.5 * nextLoopObj.Angle.Value), 5);

                noteDensity += opacity;
            }

            double noteDensityDifficulty = 0;

            if (hidden)
                noteDensityDifficulty = Math.Pow(noteDensity, 2.5) * 1.2;

            difficulty += noteDensityDifficulty;

            double preemptDifficulty = 0.0;
            if (currObj.preempt < 400)
                preemptDifficulty += Math.Pow(400 - currObj.preempt, 1.5) / 7;

            difficulty += preemptDifficulty;

            return difficulty;
        }

        private static double logistic(double x) => 1 / (1 + Math.Pow(Math.E, -x));
    }
}
