// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public class OverallStrainEvaluator
    {
        private const double release_threshold = 30;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is TailNote)
                return 0;

            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double currStartTime = maniaCurrent.StartTime;
            double currEndTime = maniaCurrent.Tail?.ActualTime ?? currStartTime;
            bool isOverlapping = false;

            double closestEndTime = Math.Abs(currEndTime - currStartTime); // Lowest value we can assume with the current information
            double holdFactor = 1.0; // Factor to all additional strains in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            // Bonus for interaction between LNs
            foreach (var maniaPrevious in maniaCurrent.PreviousHeadObjects)
            {
                if (maniaPrevious is null || !maniaPrevious.IsHold)
                {
                    // This is wrong but yea match live woo hoo
                    if (maniaPrevious is not null)
                        closestEndTime = Math.Min(closestEndTime, Math.Abs(currEndTime - maniaPrevious.StartTime));

                    continue;
                }

                // We count this note as overlapped if the current note is an LN and any other LN is arranged like this:
                // Prev  Curr
                //        O <- Current LN's tail comes after previous LN's tail
                //  O     |
                //  |     O <- Current LN's head intersects previous LN's body
                //  O
                if (maniaCurrent.IsHold)
                {
                    bool currentHeadIntersects = Precision.DefinitelyBigger(maniaPrevious.Tail!.ActualTime, currStartTime, 1)
                                                 && Precision.DefinitelyBigger(currStartTime, maniaPrevious.StartTime, 1);

                    bool currentTailComesAfter = Precision.DefinitelyBigger(maniaCurrent.Tail!.ActualTime, maniaPrevious.Tail.ActualTime, 1);

                    isOverlapping |= currentHeadIntersects && currentTailComesAfter;
                }

                // We also give a bonus to this note if an LN exists that starts before this note and ends after.
                if (Precision.DefinitelyBigger(maniaPrevious.Tail!.ActualTime, currEndTime, 1) &&
                    Precision.DefinitelyBigger(currStartTime, maniaPrevious.StartTime, 1))
                    holdFactor = 1.25;

                closestEndTime = Math.Min(closestEndTime, Math.Abs(currEndTime - maniaPrevious.Tail.ActualTime));
            }

            // The hold addition is given if there was an overlap, however it is only valid if there are no other note with a similar ending.
            // Releasing multiple notes is just as easy as releasing 1. Nerfs the hold addition by half if the closest release is release_threshold away.
            // holdAddition
            //     ^
            // 1.0 + - - - - - -+-----------
            //     |           /
            // 0.5 + - - - - -/   Sigmoid Curve
            //     |         /|
            // 0.0 +--------+-+---------------> Release Difference / ms
            //         release_threshold
            if (isOverlapping)
                holdAddition = DifficultyCalculationUtils.Logistic(x: closestEndTime, multiplier: 0.27, midpointOffset: release_threshold);

            return (1 + holdAddition) * holdFactor;
        }
    }
}
