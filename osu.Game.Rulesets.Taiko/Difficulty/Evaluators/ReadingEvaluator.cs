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
            double effectiveBPM = noteObject.EffectiveBPM;

			var midVelocity = new VelocityRange(360, 480);
            var highVelocity = new VelocityRange(480, 660);

			// High velocity notes are penalised if their note density is high
			// Density is worked out by taking the time between this note and the previous
			// and comparing it to the expected time at this note's effective BPM
			double density = (21000.0 / effectiveBPM) / noteObject.DeltaTime;
			
			// https://www.desmos.com/calculator/biltwjojyo
			double densityPenalty = DifficultyCalculationUtils.Logistic(density, 1, 15);
			
			double midVelDifficulty = 0.5 * DifficultyCalculationUtils.Logistic(effectiveBPM, midVelocity.Center, 1.0 / (midVelocity.Range / 10));
			double highVelDifficulty = (1.0 - 0.6 * densityPenalty) * DifficultyCalculationUtils.Logistic(effectiveBPM, (highVelocity.Center + 30 * densityPenalty), 0.1 * (highVelocity.Range + 0.8 * densityPenalty));
			
            return midVelDifficulty + highVelDifficulty;
        }
    }
}
