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
        private static double sigmoid(double val, double center, double width)
        {
            return Math.Tanh(Math.E * -(val - center) / width);
        }

        private static double sigmoid(double val, double center, double width, double middle, double height)
        {
            return sigmoid(val, center, width) * (height / 2) + middle;
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="MonoEncoding"/>.
        /// <param name="encoding">The encoding to evaluate.</param>
        /// <param name="i">The index of the mono encoding within it's parent <see cref="ColourEncoding"/>.</param>
        /// </summary>
        public static double EvaluateDifficultyOf(MonoEncoding encoding, int i)
        {
            return sigmoid(i, 2, 2, 0.5, 1);
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="ColourEncoding"/>.
        /// </summary>
        /// <param name="encoding">The encoding to evaluate.</param>
        /// <param name="i">The index of the colour encoding within it's parent <see cref="CoupledColourEncoding"/>.</param>
        public static double EvaluateDifficultyOf(ColourEncoding encoding, int i)
        {
            return sigmoid(i, 2, 2, 0.5, 1);
        }

        /// <summary>
        /// Evaluate the difficulty of the first note of a <see cref="CoupledColourEncoding"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(CoupledColourEncoding encoding)
        {
            return 1 - sigmoid(encoding.RepetitionInterval, 2, 2, 0.5, 1);
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
                double colourEncodingDifficulty = EvaluateDifficultyOf(colourEncoding, i) * coupledEncodingDifficulty;
                colourEncoding.Payload[0].EncodedData[0].Colour!.EvaluatedDifficulty += colourEncodingDifficulty;

                for (int j = 0; j < colourEncoding.Payload.Count; j++)
                {
                    MonoEncoding monoEncoding = colourEncoding.Payload[j];
                    monoEncoding.EncodedData[0].Colour!.EvaluatedDifficulty += EvaluateDifficultyOf(monoEncoding, j) * colourEncodingDifficulty * 0.5;
                }
            }
        }
    }
}
