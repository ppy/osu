// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            double effectiveBPM = Math.Max(1.0, noteObject.EffectiveBPM);
			// Expected deltatime is the deltatime this note would need
			// to be spaced equally to a base SV 1/4 note
			double expectedDeltaTime = 21000.0 / effectiveBPM;

			var midVelocity = new VelocityRange(360, 480);
            var highVelocity = new VelocityRange(480, 640);
			
			double midVelDifficulty = 0.5 * DifficultyCalculationUtils.Logistic(effectiveBPM, midVelocity.Center, 1.0 / (midVelocity.Range / 10));

			// Density refers to an object's deltatime relative to its expected deltatime
			double density = expectedDeltaTime / Math.Max(1.0, noteObject.DeltaTime);
			
			// Dense notes are penalised at high velocities
			// https://www.desmos.com/calculator/u63f3ntdsi
			double densityPenalty = DifficultyCalculationUtils.Logistic(density, 0.925, 15);
			
			double midpointOffset = highVelocity.Center + 8 * densityPenalty;
			double multiplier = (1.0 + 0.5 * densityPenalty) / (highVelocity.Range / 10);
			double highVelDifficulty = (1.0 - 0.33 * densityPenalty) * DifficultyCalculationUtils.Logistic(effectiveBPM, midpointOffset, multiplier);
			
            return midVelDifficulty + highVelDifficulty;
        }
    }
}
