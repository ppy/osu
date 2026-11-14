// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators.Aim
{
    public static class SpinnerEvaluator
    {
        /// <summary>
        /// Evaluates difficulty of spinning.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is not Spinner spinner || spinner.SpinsRequired <= 0)
                return 0;

            // This duration is *not* rate scaled because spinners work by applying rates on rotations.
            // This means spinning difficulty remains constant for a spinner, regardless of the applied rate.
            double duration = spinner.Duration / 1000;

            double rps = spinner.SpinsRequired / duration;

            return rps / duration; // The longer the spinner the more lenient spinning requirements are.
        }
    }
}
