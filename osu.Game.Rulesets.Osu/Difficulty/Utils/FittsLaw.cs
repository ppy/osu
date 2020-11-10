// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MathNet.Numerics;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// Helper methods related to Fitts's law - a model of human-computer interaction.
    /// Fitts's law aims to approximate human performance in a task of moving the cursor pointer
    /// to a target on the screen.
    /// https://en.wikipedia.org/wiki/Fitts%27s_law
    /// </summary>
    internal static class FittsLaw
    {
        /// <summary>
        /// Calculates the throughput (also known as the index of performance) of a single movement of the user.
        /// </summary>
        /// <remarks>
        /// The index of performance is an approximation of the user's skill in executing the movement.
        /// </remarks>
        /// <param name="distance">The distance to the target point of the movement (relative to its width).</param>
        /// <param name="movementTime">The time taken by the user to perform the movement.</param>
        public static double Throughput(double distance, double movementTime)
        {
            // uses the Shannon formulation variant:
            // https://en.wikipedia.org/wiki/Fitts%27s_law#Bits_per_second:_model_innovations_driven_by_information_theory
            // 1e-10 is a perturbing factor used to avoid division-by-zero/overflow issues
            return Math.Log(distance + 1, 2) / (movementTime + 1e-10);
        }

        /// <summary>
        /// Estimates the probability that the target is hit successfully based on the parameters given.
        /// </summary>
        /// <remarks>
        /// Model as proposed by Wobbrock, Cutrell, Harada, and MacKenzie:
        /// https://www.microsoft.com/en-us/research/wp-content/uploads/2008/04/CHI2008-Error-Model-for-Pointing.pdf
        /// </remarks>
        /// <param name="distance">The distance to the target point of the movement (relative to its width).</param>
        /// <param name="movementTime">The time taken to perform the movement.</param>
        /// <param name="throughput">The throughput of the user executing the movement to assume in the estimation.</param>
        public static double ProbabilityToHit(double distance, double movementTime, double throughput)
        {
            if (distance == 0)
                return 1.0;

            if (movementTime * throughput > 50)
                return 1.0;

            movementTime = Math.Max(movementTime, 0.03);

            return SpecialFunctions.Erf(2.066 / distance * (FastExponent.Exp2(movementTime * throughput) - 1) / Math.Sqrt(2));
        }
    }
}
