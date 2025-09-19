// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public class OverallStrainEvaluator
    {
        private const double release_threshold = 30;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double startTime = maniaCurrent.StartTime;
            double endTime = maniaCurrent.EndTime;
            bool isOverlapping = false;

            double closestEndTime = Math.Abs(endTime - startTime); // Lowest value we can assume with the current information
            double holdFactor = 1.0; // Factor to all additional strains in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            foreach (var maniaPrevious in maniaCurrent.PreviousHitObjects)
            {
                if (maniaPrevious is null)
                    continue;

                // The current note is overlapped if a previous note or end is overlapping the current note body
                isOverlapping |= Precision.DefinitelyBigger(maniaPrevious.EndTime, startTime, 1) &&
                                 Precision.DefinitelyBigger(endTime, maniaPrevious.EndTime, 1) &&
                                 Precision.DefinitelyBigger(startTime, maniaPrevious.StartTime, 1);

                // We give a slight bonus to everything if something is held meanwhile
                if (Precision.DefinitelyBigger(maniaPrevious.EndTime, endTime, 1) &&
                    Precision.DefinitelyBigger(startTime, maniaPrevious.StartTime, 1))
                    holdFactor = 1.25;

                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - maniaPrevious.EndTime));
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
