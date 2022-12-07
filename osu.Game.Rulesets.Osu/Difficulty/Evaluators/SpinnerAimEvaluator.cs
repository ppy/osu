// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public class SpinnerAimEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hasSpunOut)
        {
            if (current.BaseObject is not Spinner spinner || hasSpunOut) return 0;

            if (spinner.Duration <= 0)
                return 0;

            double r = 0.05 / (0.00008 + Math.Max(0, (5000 - spinner.Duration) / (1000 * 2000)));
            double numerator = -spinner.Duration + Math.Sqrt(spinner.Duration * spinner.Duration - 40 * r * spinner.SpinsRequired);
            double denominator = -20 * r;
            return (500000 * numerator / denominator) / spinner.Duration;
        }
    }
}
