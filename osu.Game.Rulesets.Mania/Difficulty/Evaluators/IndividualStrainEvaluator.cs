// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public class IndividualStrainEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double startTime = maniaCurrent.StartTime;
            double endTime = maniaCurrent.EndTime;

            double holdFactor = 1.0; // Factor to all additional strains in case something else is held

            for (int i = 0; i < maniaCurrent.PreviousHitObjects.Length; ++i)
            {
                if (maniaCurrent.PreviousHitObjects[i] is null)
                    continue;

                // We give a slight bonus to everything if something is held meanwhile
                if (Precision.DefinitelyBigger(maniaCurrent.PreviousHitObjects[i]!.EndTime, endTime, 1) &&
                    Precision.DefinitelyBigger(startTime, maniaCurrent.PreviousHitObjects[i]!.StartTime, 1))
                    holdFactor = 1.25;
            }

            return 2.0 * holdFactor;
        }
    }
}
