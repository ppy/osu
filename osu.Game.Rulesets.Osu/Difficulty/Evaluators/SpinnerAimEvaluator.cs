// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public class SpinnerAimEvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hasSpunOut)
        {
            if (current.BaseObject is not Spinner spinner || hasSpunOut || spinner.Duration <= 0)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            if (osuCurrent.SpinnerDuration.IsNull())
                return 0;

            double r = 0.05 / (0.00008 + Math.Max(0, (5000 - spinner.Duration) / (1000 * 2000)));
            double numerator = -spinner.Duration + Math.Sqrt(spinner.Duration * spinner.Duration - 40 * r * spinner.SpinsRequired);
            double denominator = -20 * r;

            // This value is scaled by the spinner's overall duration to account
            // for the fact that longer spinners are generally more lenient.
            return (400000 * numerator / denominator) / osuCurrent.SpinnerDuration.Value;
        }
    }
}
