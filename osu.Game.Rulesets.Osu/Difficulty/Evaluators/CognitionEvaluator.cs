// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class CognitionEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden)
        {
            var currObj = (OsuDifficultyHitObject)current;
            double noteDensity = 1.0;
            double lastOpacity = 1.0;
            int forwardsSearch = 0;

            double difficulty = 0.0;

            // This loop sucks so much lol.
            // Will be replaced in conjuction with the "objects with current visible" and the "currently visible objects" lists
            while (lastOpacity > 0)
            {
                if (currObj.Next(forwardsSearch).IsNull())
                    break;

                var searchObject = (OsuDifficultyHitObject)currObj.Next(forwardsSearch);

                lastOpacity = searchObject.OpacityAt(currObj.BaseObject.StartTime, false)
                              * logistic((searchObject.MinimumJumpDistance - 100) / 15);

                noteDensity += lastOpacity;
                forwardsSearch++;
            }

            double noteDensityDifficulty = noteDensity;

            if (hidden)
                noteDensityDifficulty = Math.Pow(noteDensity, 2.5) * 1.5;

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
