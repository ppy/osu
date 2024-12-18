// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public static class DifficultyCalculationUtils
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
    }
}
