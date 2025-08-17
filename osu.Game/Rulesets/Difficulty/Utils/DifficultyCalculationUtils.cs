// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public static partial class DifficultyCalculationUtils
    {
        /// <summary>
        /// Converts BPM value into milliseconds
        /// </summary>
        /// <param name="bpm">Beats per minute</param>
        /// <param name="delimiter">Which rhythm delimiter to use, default is 1/4</param>
        /// <returns>BPM conveted to milliseconds</returns>
        public static double BPMToMilliseconds(double bpm, int delimiter = 4)
        {
            return 60000.0 / delimiter / bpm;
        }

        /// <summary>
        /// Converts milliseconds value into a BPM value
        /// </summary>
        /// <param name="ms">Milliseconds</param>
        /// <param name="delimiter">Which rhythm delimiter to use, default is 1/4</param>
        /// <returns>Milliseconds conveted to beats per minute</returns>
        public static double MillisecondsToBPM(double ms, int delimiter = 4)
        {
            return 60000.0 / (ms * delimiter);
        }

        /// <summary>
        /// Calculates a S-shaped logistic function (https://en.wikipedia.org/wiki/Logistic_function)
        /// </summary>
        /// <param name="x">Value to calculate the function for</param>
        /// <param name="maxValue">Maximum value returnable by the function</param>
        /// <param name="multiplier">Growth rate of the function</param>
        /// <param name="midpointOffset">How much the function midpoint is offset from zero <paramref name="x"/></param>
        /// <returns>The output of logistic function of <paramref name="x"/></returns>
        public static double Logistic(double x, double midpointOffset, double multiplier, double maxValue = 1) => maxValue / (1 + Math.Exp(multiplier * (midpointOffset - x)));

        /// <summary>
        /// Calculates a S-shaped logistic function (https://en.wikipedia.org/wiki/Logistic_function)
        /// </summary>
        /// <param name="maxValue">Maximum value returnable by the function</param>
        /// <param name="exponent">Exponent</param>
        /// <returns>The output of logistic function</returns>
        public static double Logistic(double exponent, double maxValue = 1) => maxValue / (1 + Math.Exp(exponent));

        /// <summary>
        /// Returns the <i>p</i>-norm of an <i>n</i>-dimensional vector (https://en.wikipedia.org/wiki/Norm_(mathematics))
        /// </summary>
        /// <param name="p">The value of <i>p</i> to calculate the norm for.</param>
        /// <param name="values">The coefficients of the vector.</param>
        /// <returns>The <i>p</i>-norm of the vector.</returns>
        public static double Norm(double p, params double[] values) => Math.Pow(values.Sum(x => Math.Pow(x, p)), 1 / p);

        /// <summary>
        /// Calculates a Gaussian-based bell curve function (https://en.wikipedia.org/wiki/Gaussian_function)
        /// </summary>
        /// <param name="x">Value to calculate the function for</param>
        /// <param name="mean">The mean (center) of the bell curve</param>
        /// <param name="width">The width (spread) of the curve</param>
        /// <param name="multiplier">Multiplier to adjust the curve's height</param>
        /// <returns>The output of the bell curve function of <paramref name="x"/></returns>
        public static double BellCurve(double x, double mean, double width, double multiplier = 1.0) => multiplier * Math.Exp(Math.E * -(Math.Pow(x - mean, 2) / Math.Pow(width, 2)));

        /// <summary>
        /// Smoothstep function (https://en.wikipedia.org/wiki/Smoothstep)
        /// </summary>
        /// <param name="x">Value to calculate the function for</param>
        /// <param name="start">Value at which function returns 0</param>
        /// <param name="end">Value at which function returns 1</param>
        public static double Smoothstep(double x, double start, double end)
        {
            x = Math.Clamp((x - start) / (end - start), 0.0, 1.0);

            return x * x * (3.0 - 2.0 * x);
        }

        /// <summary>
        /// Smootherstep function (https://en.wikipedia.org/wiki/Smoothstep#Variations)
        /// </summary>
        /// <param name="x">Value to calculate the function for</param>
        /// <param name="start">Value at which function returns 0</param>
        /// <param name="end">Value at which function returns 1</param>
        public static double Smootherstep(double x, double start, double end)
        {
            x = Math.Clamp((x - start) / (end - start), 0.0, 1.0);

            return x * x * x * (x * (6.0 * x - 15.0) + 10.0);
        }

        /// <summary>
        /// Reverse linear interpolation function (https://en.wikipedia.org/wiki/Linear_interpolation)
        /// </summary>
        /// <param name="x">Value to calculate the function for</param>
        /// <param name="start">Value at which function returns 0</param>
        /// <param name="end">Value at which function returns 1</param>
        public static double ReverseLerp(double x, double start, double end)
        {
            return Math.Clamp((x - start) / (end - start), 0.0, 1.0);
        }
    }
}
