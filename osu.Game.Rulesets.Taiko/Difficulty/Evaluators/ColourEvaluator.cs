// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public static double Sigmoid(double val, double center, double width, double middle, double height)
        {
            double sigmoid = Math.Tanh(Math.E * -(val - center) / width);
            return sigmoid * (height / 2) + middle;
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="MonoEncoding"/> or a <see cref="ColourEncoding"/>.
        /// <param name="i">The index of either encoding within it's respective parent.</param>
        /// </summary>
        public static double EvaluateDifficultyOf(int i)
        {
            return Sigmoid(i, 2, 2, 0.5, 1);
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="CoupledColourEncoding"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(CoupledColourEncoding encoding)
        {
            return 1 - Sigmoid(encoding.RepetitionInterval, 2, 2, 0.5, 1);
        }

        /// <summary>
        /// Pre-evaluate and *assign* difficulty values of all hit objects encoded in a <see cref="CoupledColourEncoding"/>.
        /// Difficulty values are assigned to <see cref="TaikoDifficultyHitObjectColour.EvaluatedDifficulty"/> of each
        /// <see cref="TaikoDifficultyHitObject"/> encoded within.
        /// </summary>
        public static void PreEvaluateDifficulties(CoupledColourEncoding encoding)
        {
            double coupledEncodingDifficulty = 2 * EvaluateDifficultyOf(encoding);
            encoding.Payload[0].Payload[0].EncodedData[0].Colour!.EvaluatedDifficulty += coupledEncodingDifficulty;

            for (int i = 0; i < encoding.Payload.Count; i++)
            {
                ColourEncoding colourEncoding = encoding.Payload[i];
                double colourEncodingDifficulty = EvaluateDifficultyOf(i) * coupledEncodingDifficulty;
                colourEncoding.Payload[0].EncodedData[0].Colour!.EvaluatedDifficulty += colourEncodingDifficulty;

                for (int j = 0; j < colourEncoding.Payload.Count; j++)
                {
                    MonoEncoding monoEncoding = colourEncoding.Payload[j];
                    monoEncoding.EncodedData[0].Colour!.EvaluatedDifficulty += EvaluateDifficultyOf(j) * colourEncodingDifficulty * 0.5;
                }
            }
        }
    }
}
