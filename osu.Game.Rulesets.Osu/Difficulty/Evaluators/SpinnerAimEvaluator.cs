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
        /// <summary>
        /// Evaluates the difficulty of spinning the current object, based on:
        /// <list type="bullet">
        /// <item><description>the spinning velocity required to achieve a Great hitresult,</description></item>
        /// <item><description>and the duration of the spinner.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hasSpunOut)
        {
            if (current.BaseObject is not Spinner spinner || hasSpunOut || spinner.Duration <= 0)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            if (osuCurrent.SpinnerDuration.IsNull())
                return 0;

            // Desmos: https://www.desmos.com/calculator/ps8s5hghnl

            // The game permits a max spinning velocity of 0.05 units per ms, and it
            // takes some time before the maximum spinning velocity on a spinner is
            // achieved. This value is the time at which the maximum spinning velocity
            // is achieved by the player.
            double timeOfMaxSpinningVelocity = 0.05 / (0.00008 + Math.Max(0, (5000 - spinner.Duration) / (1000 * 2000)));

            // This is a quadratic equation that returns the minimum rad/ms required by
            // the player to achieve a Great (300) hit result on a spinner.
            double numerator = -spinner.Duration + Math.Sqrt(spinner.Duration * spinner.Duration - 40 * timeOfMaxSpinningVelocity * spinner.DifficultySpinsRequired);
            double denominator = -20 * timeOfMaxSpinningVelocity;

            // This value is scaled by the spinner's overall duration to account
            // for the fact that longer spinners generally play more lenient.
            return (175000 * numerator / denominator) / osuCurrent.SpinnerDuration.Value;
        }
    }
}
