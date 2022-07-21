// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class ColourEvaluator
    {
        /// <summary>
        /// A sigmoid function. It gives a value between (middle - height/2) and (middle + height/2).
        /// </summary>
        /// <param name="val">The input value.</param>
        /// <param name="center">The center of the sigmoid, where the largest gradient occurs and value is equal to middle.</param>
        /// <param name="width">The radius of the sigmoid, outside of which values are near the minimum/maximum.</param>
        /// <param name="middle">The middle of the sigmoid output.</param>
        /// <param name="height">The height of the sigmoid output. This will be equal to max value - min value.</param>
        private static double sigmoid(double val, double center, double width, double middle, double height)
        {
            double sigmoid = Math.Tanh(Math.E * -(val - center) / width);
            return sigmoid * (height / 2) + middle;
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="MonoEncoding"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(MonoEncoding encoding)
        {
            return sigmoid(encoding.Index, 2, 2, 0.5, 1) * EvaluateDifficultyOf(encoding.Parent!) * 0.5;
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="ColourEncoding"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(ColourEncoding encoding)
        {
            return sigmoid(encoding.Index, 2, 2, 0.5, 1) * EvaluateDifficultyOf(encoding.Parent!);
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="CoupledColourEncoding"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(CoupledColourEncoding encoding)
        {
            return 2 * (1 - sigmoid(encoding.RepetitionInterval, 2, 2, 0.5, 1));
        }

        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject)
        {
            TaikoDifficultyHitObjectColour colour = ((TaikoDifficultyHitObject)hitObject).Colour;
            double difficulty = 0.0d;

            if (colour.MonoEncoding != null) // Difficulty for MonoEncoding
                difficulty += EvaluateDifficultyOf(colour.MonoEncoding);
            if (colour.ColourEncoding != null) // Difficulty for ColourEncoding
                difficulty += EvaluateDifficultyOf(colour.ColourEncoding);
            if (colour.CoupledColourEncoding != null) // Difficulty for CoupledColourEncoding
                difficulty += EvaluateDifficultyOf(colour.CoupledColourEncoding);

            return difficulty;
        }
    }
}
