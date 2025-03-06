// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public static class StrainEvaluator
    {
        private const double release_threshold = 30;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double startTime = maniaCurrent.StartTime;
            double endTime = maniaCurrent.EndTime;

            double holdFactor = 0; // Factor to all additional strains in case something else is held

            var prevHitObjs = maniaCurrent.PreviousHitObjects;

            foreach (var prevObj in prevHitObjs)
            {
                if (prevObj is null)
                    continue;

                // We give a bonus for everything that is held at the same time, excluding the first 80ms.
                if (prevObj.EndTime - endTime > 1 && startTime - prevObj.StartTime > 1)
                {
                    double holdBonus = DifficultyCalculationUtils.Logistic(x: startTime - prevObj.StartTime, multiplier: 0.25, midpointOffset: 80);

                    holdFactor = DifficultyCalculationUtils.Norm(7, holdFactor, holdBonus);
                }
            }

            return 3.5 * (1 + holdFactor);
        }
    }
}
