// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
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

            var osuCurrent = (OsuDifficultyHitObject)current;

            // The average RPS required over the length of the spinner to clear the spinner.
            double rps = IBeatmapDifficultyInfo.DifficultyRange(osuCurrent.OverallDifficulty, Spinner.CLEAR_RPM_RANGE) / 60;

            double duration = spinner.Duration / current.ClockRate / 1000;

            return rps / duration; // The longer the spinner the more lenient spinning requirements are.
        }
    }
}
