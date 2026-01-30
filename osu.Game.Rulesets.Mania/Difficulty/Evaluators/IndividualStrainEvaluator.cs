// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public class IndividualStrainEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is TailNote)
                return 0;

            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double currStartTime = maniaCurrent.StartTime;
            double currEndTime = maniaCurrent.Tail?.EndTime ?? currStartTime;

            double holdFactor = 1.0; // Factor to all additional strains in case something else is held

            // We award a bonus if this note starts and ends before the end of another hold note.
            foreach (var maniaPrevious in maniaCurrent.PreviousHeadObjects)
            {
                if (maniaPrevious is null || !maniaPrevious.IsHold)
                    continue;

                if (Precision.DefinitelyBigger(maniaPrevious.Tail!.ActualTime, currEndTime, 1) &&
                    Precision.DefinitelyBigger(currStartTime, maniaPrevious.StartTime, 1))
                {
                    holdFactor = 1.25;
                    break;
                }
            }

            return 2.0 * holdFactor;
        }
    }
}
