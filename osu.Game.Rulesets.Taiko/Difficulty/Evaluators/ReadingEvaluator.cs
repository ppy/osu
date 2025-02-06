// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private readonly struct VelocityRange
        {
            public double Min { get; }
            public double Max { get; }
            public double Center => (Max + Min) / 2;
            public double Range => Max - Min;

            public VelocityRange(double min, double max)
            {
                Min = min;
                Max = max;
            }
        }

        /// <summary>
        /// Calculates the influence of higher slider velocities on hitobject difficulty.
        /// The bonus is determined based on the EffectiveBPM, shifting within a defined range
        /// between the upper and lower boundaries to reflect how increased slider velocity impacts difficulty.
        /// </summary>
        /// <param name="noteObject">The hit object to evaluate.</param>
        /// <returns>The reading difficulty value for the given hit object.</returns>
        public static double EvaluateDifficultyOf(TaikoDifficultyHitObject noteObject)
        {
            var highVelocity = new VelocityRange(480, 640);
            var midVelocity = new VelocityRange(360, 480);

            // Apply a cap to prevent outlier values on maps that exceed the editor's parameters.
            double effectiveBPM = Math.Max(1.0, noteObject.EffectiveBPM);

            double midVelocityDifficulty = 0.5 * DifficultyCalculationUtils.Logistic(effectiveBPM, midVelocity.Center, 1.0 / (midVelocity.Range / 10));

            // Expected DeltaTime is the DeltaTime this note would need to be spaced equally to a base slider velocity 1/4 note.
            double expectedDeltaTime = 21000.0 / effectiveBPM;
            double objectDensity = expectedDeltaTime / Math.Max(1.0, noteObject.DeltaTime);

            // High density is penalised at high velocity as it is generally considered easier to read. See https://www.desmos.com/calculator/u63f3ntdsi
            double densityPenalty = DifficultyCalculationUtils.Logistic(objectDensity, 0.925, 15);

            double highVelocityDifficulty = (1.0 - 0.33 * densityPenalty)
                                            * DifficultyCalculationUtils.Logistic(effectiveBPM, highVelocity.Center + 8 * densityPenalty, (1.0 + 0.5 * densityPenalty) / (highVelocity.Range / 10));

            return midVelocityDifficulty + highVelocityDifficulty;
        }
    }
}
